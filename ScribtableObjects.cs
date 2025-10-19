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
        public override bool CanExecute(GenericSkill skillSlot) => GetGooboMinionsCount(skillSlot) > 0 && base.CanExecute(skillSlot);
        public override bool IsReady(GenericSkill skillSlot) => base.IsReady(skillSlot) && GetGooboMinionsCount(skillSlot) > 0;
    }
    [CreateAssetMenu(menuName = "RoR2/SkillDef/Goobo/GooboThrowGooboMinionsSkillDef")]
    public class GooboThrowGooboMinionsSkillDef : GooboSkillDef
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
        public static bool HasTarget(GenericSkill skillSlot)
        {
            GobooThrowGooboMinionsTracker huntressTracker = skillSlot.skillInstanceData == null ? null : ((InstanceData)skillSlot.skillInstanceData).gobooThrowGooboMinionsTracker;
            return (huntressTracker != null) ? huntressTracker.trackingTarget : null;
        }
        public override bool CanExecute(GenericSkill skillSlot) => HasTarget(skillSlot) && base.CanExecute(skillSlot);
        public override bool IsReady(GenericSkill skillSlot) => base.IsReady(skillSlot) && HasTarget(skillSlot);
        public class InstanceData : BaseSkillInstanceData
        {
            public GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker;
            public GenericSkill genericSkill;
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
}
