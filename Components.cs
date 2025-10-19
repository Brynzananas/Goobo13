using HG;
using JetBrains.Annotations;
using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace Goobo13
{
    public class GobooThrowGooboMinionsTracker : MonoBehaviour
    {
        public GooboThrowGooboMinionsSkillDef.InstanceData instanceData;
        public static GameObject trackingPrefab;
        public float maxTrackingDistance = 48f;
        public float maxTrackingAngle = 45f;
        public float trackerUpdateFrequency = 10f;
        public HurtBox trackingTarget;
        public CharacterBody characterBody;
        public TeamComponent teamComponent;
        public InputBankTest inputBank;
        public float trackerUpdateStopwatch;
        public Indicator indicator;
        public BullseyeSearch search = new BullseyeSearch();
        public void Awake()
        {
            if (trackingPrefab == null)
            {
                trackingPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
            }
            indicator = new Indicator(gameObject, trackingPrefab);
        }
        public void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();
        }
        public void OnEnable()
        {
            indicator.active = true;
        }
        public void OnDisable()
        {
            indicator.active = false;
        }
        public void FixedUpdate()
        {
            MyFixedUpdate(Time.fixedDeltaTime);
        }
        public void MyFixedUpdate(float deltaTime)
        {
            if (instanceData != null && instanceData.genericSkill && instanceData.genericSkill.stock > 0)
            {
                indicator.active = true;
            }
            else
            {
                indicator.active = false;
            }
            trackerUpdateStopwatch += deltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;
                HurtBox hurtBox = trackingTarget;
                Ray ray = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                SearchForTarget(ray);
                indicator.targetTransform = (trackingTarget ? trackingTarget.transform : null);
            }
        }
        public void SearchForTarget(Ray aimRay)
        {
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(gameObject);
            foreach (HurtBox hurtBox in search.GetResults())
            {
                if (hurtBox.healthComponent && hurtBox.healthComponent.alive)
                {
                    trackingTarget = hurtBox;
                    return;
                }
            }
            trackingTarget = null;
        }
    }
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(Rigidbody))]
    public class GooboProjectrileIntoGoobos : MonoBehaviour
    {
        public static int gooboAmount = 2;
        public static float minSpread = 5f;
        public static float maxSpread = 15f;
        public ProjectileController projectileController;
        public Rigidbody rigidBody;
        public void Start()
        {
            if (!NetworkServer.active) return;
            CharacterBody ownerBody = projectileController.owner ? projectileController.owner.GetComponent<CharacterBody>() : null;
            if (!ownerBody) return;
            CharacterMaster ownerMaster = ownerBody.master;
            if (!ownerMaster) return;
            Vector3 velocity = rigidBody.velocity;
            float magnitude = velocity.magnitude;
            for (int i = 0; i < gooboAmount; i++)
            {
                CharacterMaster gooboMaster = Utils.SpawnGooboClone(ownerMaster, transform.position, Quaternion.LookRotation(velocity.normalized));
                if (!gooboMaster) continue;
                CharacterBody gooboBody = gooboMaster.GetBody();
                if (!gooboBody) continue;
                Vector3 newVelocity = Util.ApplySpread(velocity.normalized, minSpread, maxSpread, 1f, 1f) * magnitude;
                if (gooboBody.characterMotor)
                {
                    gooboBody.characterMotor.velocity = velocity;
                }
                else if (gooboBody.rigidbody)
                {
                    gooboBody.rigidbody.velocity = velocity;
                }
            }
            Destroy(gameObject);
        }
    }
    public class GooboPunchEffect : MonoBehaviour
    {
        public EffectComponent effectComponent;
        public ParticleSystem[] particleSystems;
        public void Update()
        {
            if (!effectComponent || effectComponent.effectData == null) return;
            float speed = 1f / effectComponent.effectData.genericFloat;
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.playbackSpeed = speed;
            }
        }
    }
    public class ChangeLayerOnTimer : MonoBehaviour
    {
        public CharacterBody characterBody;
        public float timer;
        public int layer;
        private bool changedCollision;
        public void FixedUpdate()
        {
            timer -= Time.fixedDeltaTime;
            if (!changedCollision && timer <= 0f)  ChangeCollsion();
        }
        public void ChangeCollsion()
        {
            changedCollision = true;
            if (!characterBody) return;
            characterBody.gameObject.layer = layer;
            if (characterBody.characterMotor && characterBody.characterMotor.Motor) characterBody.characterMotor.Motor.RebuildCollidableLayers();
            Destroy(this);
        }
    }
}
