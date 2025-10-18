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
    [CreateAssetMenu(menuName = "RoR2/SkillDef/Goobo/GooboRandomGrenadeSkillDef")]
    public class GooboRandomGrenadeSkillDef : SkillDef
    {
        public GameObject[] grenades;
        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
        {
            return new InstanceData
            {
                currentGrenade = grenades[UnityEngine.Random.Range(0, grenades.Length)]
            };
        }
        public override void OnExecute(GenericSkill skillSlot)
        {
            base.OnExecute(skillSlot);
            SelectGrenade(skillSlot);
        }
        public void SelectGrenade(GenericSkill genericSkill)
        {
            InstanceData instanceData = genericSkill.skillInstanceData == null ? null : genericSkill.skillInstanceData as InstanceData;
            if (instanceData == null) return;
            List<GameObject> grenades = [];
            int count = 0;
            foreach (GameObject grenade in this.grenades)
            {
                if (grenade == null || grenade == instanceData.currentGrenade) continue;
                count++;
                grenades.Add(grenade);
            }
            if (count <= 0) return;
            GameObject newGrenade = grenades[UnityEngine.Random.Range(0, count)];
            instanceData.currentGrenade = newGrenade;
        }
        public class InstanceData : BaseSkillInstanceData
        {
            public GameObject currentGrenade;
        }
    }
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
                CharacterMaster gooboMaster = Utils.SpawnGoobo(ownerMaster, transform.position, Quaternion.LookRotation(velocity.normalized));
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
    [CreateAssetMenu(menuName = "RoR2/SkillDef/Goobo/GooboThrowGooboMinionsSkillDef")]
    public class GooboThrowGooboMinionsSkillDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
        {
            base.OnAssigned(skillSlot);
            GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = skillSlot.GetOrAddComponent<GobooThrowGooboMinionsTracker>();
            InstanceData instanceData = new InstanceData
            {
                gobooThrowGooboMinionsTracker = gobooThrowGooboMinionsTracker,
                genericSkill = skillSlot
            };
            gobooThrowGooboMinionsTracker.instanceData = instanceData;
            return instanceData;
        }
        public override void OnUnassigned(GenericSkill skillSlot)
        {
            base.OnUnassigned(skillSlot);
            GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = skillSlot.GetComponent<GobooThrowGooboMinionsTracker>();
            if (gobooThrowGooboMinionsTracker) Destroy(gobooThrowGooboMinionsTracker);
        }
        public override void OnFixedUpdate(GenericSkill skillSlot, float deltaTime)
        {
            base.OnFixedUpdate(skillSlot, deltaTime);
            skillSlot.stock = GetGooboMinionsCount(skillSlot);
        }
        public static int GetGooboMinionsCount(GenericSkill skillSlot)
        {
            CharacterMaster characterMaster = skillSlot.characterBody?.master;
            if (characterMaster)
            {
                return characterMaster.GetDeployableCount(Assets.GooboDeployableSlot);
            }
            else
            {
                return 0;
            }
        }
        public static bool HasTarget(GenericSkill skillSlot)
        {
            GobooThrowGooboMinionsTracker huntressTracker = skillSlot.skillInstanceData == null ? null : ((InstanceData)skillSlot.skillInstanceData).gobooThrowGooboMinionsTracker;
            return (huntressTracker != null) ? huntressTracker.trackingTarget : null;
        }
        public override bool CanExecute(GenericSkill skillSlot) => HasTarget(skillSlot) && skillSlot.stock > 0 && base.CanExecute(skillSlot);
        public override bool IsReady(GenericSkill skillSlot) => base.IsReady(skillSlot) && HasTarget(skillSlot);
        public class InstanceData : BaseSkillInstanceData
        {
            public GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker;
            public GenericSkill genericSkill;
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
    
}
