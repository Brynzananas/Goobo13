using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Goobo13
{
    [CreateAssetMenu(menuName = "RoR2/SkillDef/Goobo/GooboSkillDef")]
    public class GooboSkillDef : SkillDef
    {
        [Header("Goobo Parameters")]
        public bool tracking;
        public bool scaleStocksWithGooboMinions;
        public bool canExecuteTargetCheck;
        public bool isReadyTargetCheck;
        public bool canExecuteGooboMinionsCheck;
        public bool isReadyGooboMinionsCheck;
        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
        {
            BaseSkillInstanceData baseSkillInstanceData = base.OnAssigned(skillSlot);
            if (!tracking) return baseSkillInstanceData;
            GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = skillSlot.GetOrAddComponent<GobooThrowGooboMinionsTracker>();
            gobooThrowGooboMinionsTracker.activeCount++;
            InstanceData instanceData = new InstanceData
            {
                gobooThrowGooboMinionsTracker = gobooThrowGooboMinionsTracker
            };
            return instanceData;
        }
        public override void OnUnassigned(GenericSkill skillSlot)
        {
            base.OnUnassigned(skillSlot);
            if (!tracking) return;
            GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = skillSlot.GetComponent<GobooThrowGooboMinionsTracker>();
            if (gobooThrowGooboMinionsTracker) gobooThrowGooboMinionsTracker.activeCount--;
        }
        public override void OnFixedUpdate(GenericSkill skillSlot, float deltaTime)
        {
            base.OnFixedUpdate(skillSlot, deltaTime);
            if (!scaleStocksWithGooboMinions) return;
            skillSlot.stock = GetGooboMinionsCount(skillSlot);
        }
        public static bool HasTarget(GenericSkill skillSlot)
        {
            GobooThrowGooboMinionsTracker gooboTracker = skillSlot.skillInstanceData == null ? null : ((InstanceData)skillSlot.skillInstanceData).gobooThrowGooboMinionsTracker;
            return (gooboTracker != null) ? gooboTracker.trackingTarget : null;
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
        public override bool CanExecute(GenericSkill skillSlot) => (canExecuteGooboMinionsCheck ? GetGooboMinionsCount(skillSlot) > 0 : true) && base.CanExecute(skillSlot) && (canExecuteTargetCheck ? HasTarget(skillSlot) : true);
        public override bool IsReady(GenericSkill skillSlot) => (isReadyGooboMinionsCheck ? GetGooboMinionsCount(skillSlot) > 0 : true) && base.IsReady(skillSlot) && (isReadyTargetCheck ? HasTarget(skillSlot) : true);
        public class InstanceData : BaseSkillInstanceData
        {
            public GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker;
        }
    }
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
    public class RevolutionarySkillDef : SkillDef
    {
        [Header("Revolutionary Parameters")]
        public bool transplanar;
        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            base.OnExecute(skillSlot);
            if (transplanar)
            {
                if (Utils.GetRandomNodePosition(out Vector3 nodePosition))
                {
                    Utils.SpawnEvilGooboClone(nodePosition, Quaternion.identity);
                }
            }
        }
    }
}
