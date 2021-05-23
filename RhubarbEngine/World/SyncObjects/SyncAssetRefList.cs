using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public class SyncAssetRefList<T> : Worker, IWorldObject where T : IAsset
    {
        private List<AssetRef<T>> _syncreflist = new List<AssetRef<T>>();

        AssetRef<T> this[int i]
        {
            get
            {
                return _syncreflist[i];
            }
        }

        public IEnumerator<AssetRef<T>> GetEnumerator()
        {
            for (int i = 0; i < _syncreflist.Count; i++)
            {
                yield return this[i];
            }
        }

        private void onLoad(T val)
        {
            loadChange?.Invoke(val);
        }

        public Action<T> loadChange;


        public AssetRef<T> Add(bool RefID = true)
        {
            AssetRef<T> a = new AssetRef<T>(this, RefID);
            a.loadChange += onLoad;
            _syncreflist.Add(a);
            return _syncreflist[_syncreflist.Count - 1];
        }

        public void Clear()
        {
            _syncreflist.Clear();
        }

        public SyncAssetRefList(IWorldObject _parent,bool newref = true) : base(_parent.World, _parent, newref)
        {

        }

        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach(AssetRef<T> val in _syncreflist)
            {
                list.Add(val.serialize());
            }
            obj.setValue("list", list);
            return obj;
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<NetPointer, NetPointer> newRefID = default(Dictionary<NetPointer, NetPointer>), Dictionary<NetPointer, RefIDResign> latterResign = default(Dictionary<NetPointer, RefIDResign>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<NetPointer>)data.getValue("referenceID")).Value, referenceID);
                latterResign[((DataNode<NetPointer>)data.getValue("referenceID")).Value](referenceID);
            }
            else
            {
                referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            foreach (DataNodeGroup val in ((DataNodeList)data.getValue("list")))
            {
                Add(NewRefIDs).deSerialize(val, NewRefIDs, newRefID, latterResign);
            }
        }
    }
}
