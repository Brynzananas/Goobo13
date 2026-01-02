using HG;
using JetBrains.Annotations;
using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;

namespace Goobo13
{
    public class GobooThrowGooboMinionsTracker : MonoBehaviour
    {
        public List<GooboSkillDef.InstanceData> instanceDatas = [];
        public int activeCount;
        public static float maxTrackingDistance => TrackerConfig.maxDistance.Value;
        public static float maxTrackingAngle => TrackerConfig.maxAngle.Value;
        public static float trackerUpdateFrequency = 10f;
        public HurtBox trackingTarget;
        public CharacterBody characterBody;
        public TeamComponent teamComponent;
        public InputBankTest inputBank;
        public float trackerUpdateStopwatch;
        public BullseyeSearch search = new BullseyeSearch();
        public void Awake()
        {
        }
        public void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();
        }
        public void OnEnable()
        {
            foreach (GooboSkillDef.InstanceData instanceData in instanceDatas) instanceData.indicator?.active = true;
        }
        public void OnDisable()
        {
            foreach (GooboSkillDef.InstanceData instanceData in instanceDatas) instanceData.indicator?.active = false;
        }
        public void FixedUpdate()
        {
            MyFixedUpdate(Time.fixedDeltaTime);
        }
        public void MyFixedUpdate(float deltaTime)
        {
            trackerUpdateStopwatch += deltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;
                Ray ray = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                SearchForTarget(ray);
            }
            if (trackingTarget)
            {
                foreach (GooboSkillDef.InstanceData instanceData in instanceDatas)
                {
                    Indicator indicator = instanceData.indicator;
                    if (indicator == null) continue;
                    indicator.targetTransform = trackingTarget.transform;
                    indicator.active = instanceData.genericSkill.CanExecute();
                }
            }
            else
            {
                foreach (GooboSkillDef.InstanceData instanceData in instanceDatas)
                {
                    Indicator indicator = instanceData.indicator;
                    if (indicator == null) continue;
                    indicator.targetTransform = null;
                    indicator.active = false;
                }
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
            bool sort = TrackerConfig.sortByPriority.Value;
            int priority = 0;
            foreach (HurtBox hurtBox in search.GetResults())
            {
                HealthComponent healthComponent = hurtBox.healthComponent;
                if (!sort)
                {
                    if (healthComponent.alive)
                    {
                        trackingTarget = hurtBox;
                        return;
                    }
                    continue;
                }
                if (healthComponent == null) continue;
                CharacterBody characterBody = healthComponent.body;
                if (characterBody == null) continue;
                int newPriority = characterBody.isBoss ? 3 : (characterBody.isElite ? 2 : 1);
                if (newPriority > priority && healthComponent.alive)
                {
                    trackingTarget = hurtBox;
                    priority = newPriority;
                }
            }
            if (priority > 0) return;
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
    public class RevolutionaryController : MonoBehaviour
    {
        public CharacterBody characterBody;
        public CharacterBody currentTarget;
        public Indicator indicator;
        public void Awake()
        {
            indicator = new Indicator(gameObject, Assets.GooboCloneMissileTrackingIndicator);
        }
        public void OnEnable()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if (obj.attacker && obj.attacker == gameObject && obj.damageInfo.HasModdedDamageType(Assets.AbysstouchedDamageType)) currentTarget = obj.victimBody;
        }

        public void OnDisable()
        {
            GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
        }
        public void FixedUpdate()
        {
            if (currentTarget && currentTarget.mainHurtBox)
            {
                indicator.targetTransform = currentTarget.mainHurtBox.transform;
                indicator.active = true;
            }
            else
            {
                indicator.targetTransform = null;
                indicator.active = false;
            }
        }
    }
    public class ForceUpdatePostProcessLayer : MonoBehaviour
    {
        public void Start()
        {
            foreach (Camera camera in Camera.allCameras)
            {
                PostProcessLayer postProcessLayer = camera.GetComponent<PostProcessLayer>();
                if (!postProcessLayer) continue;
                PostProcessVolume postProcessVolume = camera.GetComponent<PostProcessVolume>();
            }
        }
    }
    public class SelectTargetsToDimension : MonoBehaviour
    {
        public List<TemporaryOverlay> temporaryOverlays = [];
        public void OnTriggerEnter(Collider collider)
        {
            CharacterBody characterBody = collider.GetComponent<CharacterBody>();
            if (characterBody == null) return;
            ModelLocator modelLocator = characterBody.modelLocator;
            if (modelLocator == null) return;
            Transform modelTransform = modelLocator.modelTransform;
            if (modelTransform == null) return;
            CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
            if (characterModel == null) return;
            TemporaryOverlay temporaryOverlay = characterBody.gameObject.AddComponent<TemporaryOverlay>();
            temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            temporaryOverlay.inspectorCharacterModel = characterModel;
            temporaryOverlay.originalMaterial = Assets.EnterDimensionOverlay;
            temporaryOverlay.AddToCharacerModel(characterModel);
            temporaryOverlays.Add(temporaryOverlay);
        }
        public void OnTriggerExit(Collider collider)
        {
            TemporaryOverlay temporaryOverlay = collider.GetComponent<TemporaryOverlay>();
            if (temporaryOverlay == null) return;
            if (temporaryOverlays.Contains(temporaryOverlay))
            {
                temporaryOverlays.Remove(temporaryOverlay);
                Destroy(temporaryOverlay);
            }
        }
        public void OnDestroy()
        {
            foreach (var overlay in temporaryOverlays)
            {
                if (!overlay) continue;
                Destroy(overlay);
            }
        }
    }
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(ProjectileDamage))]
    public class SpawnFartsController : MonoBehaviour
    {
        public ProjectileController projectileController;
        public ProjectileDamage projectileDamage;
        public List<SpawnFartsController> banned = [];
        public static FixedConditionalWeakTable<GameObject, List<SpawnFartsController>> activeFarts = [];
        private bool noBitches;
        private bool farting;
        public void Start()
        {
            noBitches = !(projectileController ? projectileController.owner : false);
            if (noBitches) return;
            if (activeFarts.ContainsKey(projectileController.owner))
            {
                activeFarts[projectileController.owner].Add(this);
            }
            else
            {
                activeFarts.Add(projectileController.owner, [this]);
            }
        }
        public void OnDisable()
        {
            if (!projectileDamage || !projectileController || noBitches || !projectileController.owner || !activeFarts.ContainsKey(projectileController.owner)) return;
            List<SpawnFartsController> spawnFartsControllers = activeFarts[projectileController.owner];
            spawnFartsControllers.Remove(this);
            bool isAnyoneFarting = false;
            do
            {
                isAnyoneFarting = false;
                for (int i = 0; i < spawnFartsControllers.Count; ++i)
                {
                    SpawnFartsController spawnFartsController = spawnFartsControllers[i];
                    if (spawnFartsController == null || spawnFartsController == this) continue;
                    if (banned.Contains(spawnFartsController)) continue;
                    DamageTypeCombo damageTypeCombo = new DamageTypeCombo(projectileDamage.damageType.damageType, projectileDamage.damageType.damageTypeExtended, projectileDamage.damageType.damageSource);
                    //damageTypeCombo.AddModdedDamageType(Assets.AbysstouchedDamageType);
                    Vector3 vector3 = spawnFartsController.transform.position - transform.position;
                    float fartDistance = vector3.magnitude;
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = Assets.RevolutionaryLingeringSanguineVapor,
                        position = transform.position,
                        rotation = Util.QuaternionSafeLookRotation(vector3.normalized),
                        owner = projectileController.owner,
                        damage = projectileDamage.damage,
                        force = projectileDamage.force,
                        crit = projectileDamage.crit,
                        damageTypeOverride = new DamageTypeCombo?(damageTypeCombo),
                        speedOverride = fartDistance,
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    isAnyoneFarting = true;
                    banned.Add(spawnFartsController);
                    spawnFartsController.farting = true;
                    if (!farting) Destroy(spawnFartsController.gameObject);
                }
            } while (isAnyoneFarting);
            
        }
    }
    public class DeadlyFartController : MonoBehaviour, IProjectileSpeedModifierHandler
    {
        public float range;
        public float radius;
        public float lifetime;
        public float stopwatch;
        public ParticleSystem[] particleSystems;
        public float desiredForwardSpeed { set => range = value; }
        public void Start()
        {
            Vector3 localScale = new Vector3(radius, radius, range);
            Vector3 localPosition = transform.localPosition;
            localPosition.z += range / 2f;
            transform.localPosition = localPosition;
            transform.localScale = localScale;
            localScale *= 2f;
            foreach (var particle in particleSystems)
            {
                ParticleSystem.ShapeModule shapeModule = particle.shape;
                shapeModule.scale = localScale;
            }
        }
        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= lifetime)
            {
                Destroy(gameObject);
            }
        }
        public void OnDisable()
        {
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.transform.SetParent(null, true);
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                particleSystem.transform.localScale = Vector3.one;
            }
        }
    }
    public class DestroyOnParticleEndAndNoParticles : MonoBehaviour
    {
        public void Update()
        {
            if (trackedParticleSystem && !trackedParticleSystem.IsAlive() && trackedParticleSystem.particleCount <= 0)  GameObject.Destroy(gameObject);
        }
        public ParticleSystem trackedParticleSystem;
    }
    public class ChainProjectile : MonoBehaviour
    {
        public float age;
        public List<Chain> chains = [];
        public void FixedUpdate()
        {
            age += Time.fixedDeltaTime;
            foreach (Chain chain in chains) HandleChains(chain);
        }
        public void HandleChains(Chain chain)
        {
            List<Transform> transforms = chain.transforms;
            Vector3 direction = transform.rotation * chain.direction;
            for (int i = 0; i < transforms.Count; i++)
            {
                Transform t = transforms[i];
                Vector3 vector3 = transform.position + direction;
                t.position = transform.position + direction * chain.distancePerAge * chain.ageCurve.Evaluate(age) * (i + 1);
            }
        }
        [Serializable]
        public struct Chain
        {
            public List<Transform> transforms;
            public Vector3 direction;
            public float distancePerAge;
            public AnimationCurve ageCurve;
        }
    }
    public class ScaleBetweenTransforms : MonoBehaviour
    {
        public Transform transform1;
        public Transform transform2;
        public void FixedUpdate()
        {
            Vector3 vector3 = transform2.position - transform1.position;
            transform.rotation = Quaternion.LookRotation(vector3.normalized);
            transform.position = transform1.position + vector3 / 2f;
            Vector3 localScale = transform.localScale;
            localScale.z = vector3.magnitude;
            transform.localScale = localScale;
        }
    }
    public class EnableDisableColliderAfterTime : MonoBehaviour
    {
        public Collider collider;
        public float timeToEnable;
        private float stopwatch;
        private bool enable;
        private bool done;
        public void Start()
        {
            enable = !collider.enabled;
        }

        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (!done && stopwatch >= timeToEnable)
            {
                collider.enabled = enable;
                done = true;
                Destroy(this);
            }
        }
    }
    [RequireComponent(typeof(VehicleSeat))]
    public class GooboBallVehicleController : MonoBehaviour
    {
        public Rigidbody rigidbody;
        public VehicleSeat vehicleSeat;
        public GameObject effectOnExit;
        public GameObject effectOnCollision;
        public float effectScale;
        public float effectOnCollisionScale;
        public int bounceAmount;
        public R2API.AddressReferencedAssets.AddressReferencedBuffDef buffDef;
        [HideInInspector] public Ball ball;
        private int bounceCount;
        public void Awake()
        {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        public void Start()
        {
            CharacterBody characterBody = vehicleSeat.currentPassengerBody;
            if (!characterBody) return;
            NetworkStateMachine networkStateMachine = characterBody.GetComponent<NetworkStateMachine>();
            if (!networkStateMachine || networkStateMachine.stateMachines == null) return;
            foreach (EntityStateMachine entityStateMachine in networkStateMachine.stateMachines)
            {
                if (!entityStateMachine || entityStateMachine.state == null) continue;
                if (entityStateMachine.state is Ball ball && !ball.stateTaken)
                {
                    ball.stateTaken = true;
                    this.ball = ball;
                    break;
                }
            }

        }
        public void OnEnable()
        {
            vehicleSeat.onPassengerEnter += VehicleSeat_onPassengerEnter;
            vehicleSeat.onPassengerExit += VehicleSeat_onPassengerExit;
        }

        private void VehicleSeat_onPassengerExit(GameObject obj)
        {
            if (bounceCount > bounceAmount && ball != null) ball.Explode(transform.position);
            /*EffectData effectData = new()
            {
                origin = transform.position,
                scale = effectScale,
            };
            EffectManager.SpawnEffect(effectOnExit, effectData, false);*/
            if (NetworkServer.active) Destroy(gameObject);
        }

        private void VehicleSeat_onPassengerEnter(GameObject obj)
        {

        }
        public void FixedUpdate()
        {
            if (rigidbody && ball.isAuthority && ball.inputBank)
            {
                Vector3 vector3 = rigidbody.velocity;
                vector3.y = 0f;
                vector3 = Vector3.MoveTowards(vector3, ball.inputBank.moveVector * ball.speed, ball.airControl * Time.fixedDeltaTime);
                vector3.y = rigidbody.velocity.y;
                rigidbody.velocity = vector3;
            }
            if (!vehicleSeat || !vehicleSeat.currentPassengerBody) return;
            if (NetworkServer.active && buffDef && buffDef.Asset && !vehicleSeat.currentPassengerBody.HasBuff(buffDef.Asset)) vehicleSeat.currentPassengerBody.AddBuff(buffDef.Asset);
        }
        public void OnCollisionEnter(Collision collision)
        {
            /*EffectData effectData = new()
            {
                origin = transform.position,
                scale = effectOnCollisionScale,
            };
            EffectManager.SpawnEffect(effectOnCollision, effectData, false);*/
            bounceCount++;
            if (ball != null && ball.isAuthority)
            {
                ball.Explode(collision.contacts[0].point);
                if (ball.inputBank && rigidbody)
                {
                    rigidbody.velocity = collision.contacts[0].normal * ball.jump + ball.inputBank.moveVector * ball.speed + Physics.gravity * -0.2f;
                }
            }
            if (bounceCount <= bounceAmount || !NetworkServer.active || !vehicleSeat) return;
            vehicleSeat.CallCmdEjectPassenger();
        }
        public void OnDestroy()
        {
            if (ball == null || !ball.isAuthority) return;
            ball.outer.SetNextStateToMain();
        }
    }
    /*
    public class InflateCollider : MonoBehaviour
    {
        public Collider collider;
        public float inflationTime;
        private float endSize;
        private float stopwatch;
        public void Start()
        {
            SphereCollider sphereCollider = collider as SphereCollider;
            if (sphereCollider)
            {
                endSize = sphereCollider.radius;
                return;
            }
            CapsuleCollider capsuleCollider = collider as CapsuleCollider;
            if (capsuleCollider)
            {
                endSize = capsuleCollider.radius;
                return;
            }
        }
        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            SphereCollider sphereCollider = collider as SphereCollider;
            CapsuleCollider capsuleCollider = collider as CapsuleCollider;
        }
    }*/
}
