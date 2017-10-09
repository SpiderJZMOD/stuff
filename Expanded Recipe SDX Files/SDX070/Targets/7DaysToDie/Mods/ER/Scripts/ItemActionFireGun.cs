using System;
using UnityEngine;

class ItemActionFireGun : ItemActionRanged
{
    static System.Random rnd = new System.Random();

    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        Vector3 vector3;
        Vector3 vector31;
        ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
        if (_bReleased)
        {
            itemActionDataRanged.bReleased = true;
            this.LJ(itemActionDataRanged, _actionData.indexInEntityOfAction);
            return;
        }
        if (!this.AutoFire && !itemActionDataRanged.bReleased)
        {
            return;
        }
        itemActionDataRanged.bReleased = false;
        if (itemActionDataRanged.needReloadingTime > 0f)
        {
            return;
        }
        if (Time.time - itemActionDataRanged.m_LastShotTime < this.Delay)
        {
            return;
        }
        itemActionDataRanged.m_LastShotTime = Time.time;
        EntityAlive entityAlive = _actionData.invData.holdingEntity;
        if (itemActionDataRanged.invData.itemValue.MaxUseTimes > 0 && itemActionDataRanged.invData.itemValue.UseTimes >= itemActionDataRanged.invData.itemValue.MaxUseTimes || itemActionDataRanged.invData.itemValue.UseTimes == 0 && itemActionDataRanged.invData.itemValue.MaxUseTimes == 0)
        {
            if (!this.item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
            {
                GameManager.Instance.ShowTooltip("ttItemNeedsRepair");
            }
            else
            {
                GameManager.Instance.ShowTooltipWithAlert("ttItemNeedsRepair", this.item.Properties.Values[ItemClass.PropSoundJammed]);
            }
            return;
        }
        if (!this.InfiniteAmmo && itemActionDataRanged.invData.itemValue.Meta <= 0)
        {
            if (itemActionDataRanged.needReloadingTime <= 0f)
            {
                entityAlive.PlayOneShot(this.soundEmpty);
            }
            return;
        }
        itemActionDataRanged.state = (itemActionDataRanged.state != ItemActionFiringState.Off ? ItemActionFiringState.Loop : ItemActionFiringState.Start);
        if (!this.InfiniteAmmo)
        {
            ItemValue meta = _actionData.invData.itemValue;
            meta.Meta = meta.Meta - 1;

            if (this.Properties.Values.ContainsKey("EjectCases"))
            {
                GetEjection(_actionData);                
            }
        }
        if (itemActionDataRanged.invData.itemValue.MaxUseTimes > 0)
        {
            ItemValue itemValue = _actionData.invData.itemValue;
            ItemValue useTimes = itemValue;
            useTimes.UseTimes = useTimes.UseTimes + AttributeBase.GetVal<AttributeDegradationRate>(itemActionDataRanged.invData.itemValue, 1);
            itemActionDataRanged.invData.itemValue = itemValue;
            if (itemActionDataRanged.invData.itemValue.MaxUseTimes > 0 && itemActionDataRanged.invData.itemValue.UseTimes >= itemActionDataRanged.invData.itemValue.MaxUseTimes || itemActionDataRanged.invData.itemValue.UseTimes == 0 && itemActionDataRanged.invData.itemValue.MaxUseTimes == 0)
            {
                itemActionDataRanged.state = ItemActionFiringState.Off;
            }
        }
        int modelLayer = entityAlive.GetModelLayer();
        entityAlive.SetModelLayer(2);
        Vector3 vector32 = Vector3.zero;
        if (this.MagazineItemRayCount == null || _actionData.invData.itemValue.SelectedAmmoTypeIndex >= (int)this.MagazineItemRayCount.Length)
        {
            for (int i = 0; i < this.RaysPerShot; i++)
            {
                vector32 = this.fireShot(i, itemActionDataRanged);
            }
        }
        else
        {
            for (int j = 0; j < this.MagazineItemRayCount[_actionData.invData.itemValue.SelectedAmmoTypeIndex]; j++)
            {
                vector32 = this.fireShot(j, itemActionDataRanged);
            }
        }
        entityAlive.SetModelLayer(modelLayer);
        this.getImageActionEffectsStartPosAndDirection(_actionData, out vector3, out vector31);
        itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(entityAlive.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, (int)itemActionDataRanged.state, vector3, vector31);
        if (itemActionDataRanged.invData.itemValue.Meta == 0)
        {
            itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(entityAlive.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero);
            itemActionDataRanged.state = ItemActionFiringState.Off;
            if (this.CanReload(itemActionDataRanged))
            {
                itemActionDataRanged.invData.gameManager.ItemReloadServer(entityAlive.entityId, 0.75f);
            }
        }
        Vector3 kickbackForce = base.GetKickbackForce(vector32);
        EntityAlive entityAlive1 = entityAlive;
        entityAlive1.motion = entityAlive1.motion + (kickbackForce * (!entityAlive.AimingGun ? 0.5f : 0.2f));
    }

    private void LJ(ItemActionRanged.ItemActionDataRanged itemActionDataRanged, int num)
    {
        if (itemActionDataRanged.state == ItemActionFiringState.Loop || itemActionDataRanged.state == ItemActionFiringState.Start)
        {
            itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(itemActionDataRanged.invData.holdingEntity.entityId, itemActionDataRanged.invData.slotIdx, num, 0, Vector3.zero, Vector3.zero);
            itemActionDataRanged.state = ItemActionFiringState.Off;
        }
    }

    private void GetEjection(ItemActionData _actionData)
    {
        int EC;
        if (this.Properties.Values.ContainsKey("EjectionChance"))
        {
            EC = Convert.ToInt32(Convert.ToDouble(this.Properties.Values["EjectionChance"]) * 100);
        }
        else
        {
            EC = 25;
        }

        int roll = rnd.Next(1, 101);

        if (roll <= EC)
        {
            string casingType = this.Properties.Values["EjectCases"];
            ItemStack casing = new ItemStack(ItemClass.GetItem(casingType), 1);
            EntityPlayer ePlayer = (EntityPlayer)_actionData.invData.holdingEntity;

            if (!ePlayer.bag.AddItem(casing))
            {
                if (!ePlayer.inventory.AddItem(casing))
                {
                    GameManager.Instance.ItemDropServer(casing, _actionData.invData.holdingEntity.GetPosition(), Vector3.zero, -1, 60f);
                }                
            }
        }      
    }

}

