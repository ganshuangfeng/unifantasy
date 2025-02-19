using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using UniFan;

namespace UniFan.Res
{
    public class ResourcesRes : Res
    {
        private ResourceRequest _ResourceRequest;

        public override ResType ResType => ResType.Resource;

        public static ResourcesRes Create(string assetName)
        {
            ResourcesRes res = ClassPool.Get<ResourcesRes>();
            res.AssetName = assetName;
            return res;
        }

        public override void OnReset()
        {
            base.OnReset();
            _ResourceRequest = null;
        }

        public override void Put2Pool()
        {
            ClassPool.Put<ResourcesRes>(this);
        }

        public override bool Load()
        {
            if (!CheckLoadAble())
            {
                WarnCancelSyncLoad();
                return false;
            }

            if (string.IsNullOrEmpty(AssetName))
            {
                return false;
            }

            State = ResState.Loading;

            Object obj = Resources.Load(AssetName);

            if (obj == null)
            {
                Debug.LogError("Failed to Load Asset From Resources:" + AssetName);
                OnResLoadFaild();
                return false;
            }

            Asset = obj;

            State = ResState.Ready;
            return true;
        }

        public override void LoadAsync()
        {
            if (!CheckLoadAble())
            {
                return;
            }

            if (string.IsNullOrEmpty(AssetName))
            {
                OnResLoadFaild();
                return;
            }

            State = ResState.Loading;

            ResManager.Instance.PushIEnumeratorTask(this);
        }

        public override IEnumerator DoIEnumeratorTask(System.Action finishCallback)
        {
            if (RefCount <= 0)
            {
                OnResLoadFaild();
                finishCallback();
                yield break;
            }

            var resourceRequest = Resources.LoadAsync(AssetName);

            _ResourceRequest = resourceRequest;
            yield return resourceRequest;
            _ResourceRequest = null;

            if (!resourceRequest.isDone)
            {
                Debug.LogError("Failed to Load Resources:" + AssetName);
                OnResLoadFaild();
                finishCallback();
                yield break;
            }

            Asset = resourceRequest.asset;

            State = ResState.Ready;

            finishCallback();
        }

        protected override float CalculateProgress()
        {
            if (_ResourceRequest == null)
            {
                return 0;
            }
            return _ResourceRequest.progress;
        }
    }
}