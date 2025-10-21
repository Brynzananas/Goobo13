using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Goobo13
{
    public class AddSlamSkillOverride : INetMessage
    {
        public NetworkInstanceId instanceId;
        public AddSlamSkillOverride()
        {

        }
        public AddSlamSkillOverride(NetworkInstanceId networkInstanceId)
        {
            this.instanceId = networkInstanceId;
        }
        public void Deserialize(NetworkReader reader)
        {
            reader.ReadNetworkId();
        }
        public void OnReceived()
        {
            GameObject gameObject = Util.FindNetworkObject(instanceId);
            if (!gameObject) return;
            CharacterBody characterBody = gameObject.GetComponent<CharacterBody>();
            if (!characterBody) return;
            SkillLocator skillLocator = characterBody.skillLocator;
            if (!skillLocator) return;
            GenericSkill genericSkill = skillLocator.primary;
            if (!genericSkill) return;
            genericSkill.SetSkillOverride(characterBody.gameObject, Assets.GooboSlam, GenericSkill.SkillOverridePriority.Contextual);
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(instanceId);
        }
    }
}
