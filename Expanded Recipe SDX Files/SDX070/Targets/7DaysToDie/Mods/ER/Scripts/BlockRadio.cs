
using UnityEngine;

public class BlockRadio : Block
{

    MusicPlayer script = null;
    UnityEngine.GameObject gameObject = null;

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
        gameObject = _ebcd.transform.gameObject;

        script = gameObject.GetComponent<MusicPlayer>();

        Debug.Log("             !!!!!!!!!!!!!!          OnBlockEntityTransformAfterActivated");
        if (script == null)
        {
            script = gameObject.AddComponent<MusicPlayer>();

        }

        if (script != null)
        {
            System.IO.BinaryReader file = HalHelper.GetReaderForBlockFile(_blockPos);
            if (file != null)
                script.Read(file);
            else
            {
                script.FirstTime();
            }
        }

    }
    //public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    //{
    //    base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
    //    Debug.Log("             !!!!!!!!!!!!!!          OnBlockAdded");

    //    if (gameObject == null)
    //    {

    //        BlockEntityData _ecd = new BlockEntityData(_blockValue, _blockPos);
    //        _ecd.bNeedsTemperature = true;
    //        _chunk.AddEntityBlockStub(_ecd);
    //        gameObject = _ecd.transform.gameObject;
    //    }

    //    if (script == null)
    //    {
    //        script = gameObject.AddComponent<MusicPlayer>();

    //    }

    //    if (script != null)
    //    {
    //        System.IO.BinaryReader file = HalHelper.GetReaderForBlockFile(_blockPos);
    //        if (file != null)
    //            script.Read(file);
    //        else
    //        {
    //            script.FirstTime();
    //        }
    //    }
    //}

    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        return base.OnBlockActivated(_indexInBlockActivationCommands, _world, _cIdx, _blockPos, _blockValue, _player);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {

      //  Debug.Log("             !!!!!!!!!!!!!!          OnBlockLoaded");

        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
    }

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        base.PlaceBlock(_world, _result, _ea);
    }
    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
        HalHelper.DeleteSaveFileForBlock(_blockPos);
    }

    public override void OnBlockValueChanged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
    {
        base.OnBlockValueChanged(_world, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        System.IO.BinaryWriter file = HalHelper.GetWriterForBlockFile(_blockPos);
        script.Write(file);

        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
    }

}

