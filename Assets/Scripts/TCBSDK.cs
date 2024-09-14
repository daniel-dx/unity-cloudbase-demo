

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace TencentCloud.CloudBase
{

    public class TCBSDK : MonoBehaviour
    {

        private static readonly Lazy<TCBSDK> _instance = new Lazy<TCBSDK>(() =>
        {
            GameObject gameObject = new(Internal.CONST_GAME_OBJECT_NAME);
            TCBSDK tcbSDK = gameObject.AddComponent<TCBSDK>();
            return tcbSDK;
        });
        public static TCBSDK Instance => _instance.Value;
        private TCBSDK() { }

        #region 对外暴露的方法

        /// <summary>
        /// 初始化云开发 Client
        /// </summary>
        public async Task<IApp> Init(CloudInitParams input)
        {
            (string, TaskCompletionSource<string>) asyncTask = Internal.GetAsyncTask();

            Internal.CloudInit(asyncTask.Item1, Internal.ParseInputParams(input));

            await asyncTask.Item2.Task;
            return App.Instance;
        }

        /// <summary>
        /// 匿名登录
        /// </summary>
        public async Task Auth_SignInAnonymously()
        {
            (string, TaskCompletionSource<string>) asyncTask = Internal.GetAsyncTask();

            Internal.Auth_SignInAnonymously(asyncTask.Item1);

            await asyncTask.Item2.Task;
            return;
        }

        /// <summary>
        /// 获取用户 UUID
        /// </summary>
        public string GetUserUUID()
        {
            return Internal.GetUserUUID();
        }

        /// <summary>
        /// 调用云函数
        /// </summary>
        public async Task<CallFunctionResponse<T>> CallFunction<T>(CallFunctionParams input)
        {
            (string, TaskCompletionSource<string>) asyncTask = Internal.GetAsyncTask();

            Internal.CallFunction(asyncTask.Item1, Internal.ParseInputParams(input));

            string result = await asyncTask.Item2.Task;
            return Internal.ParseOutputResult<CallFunctionResponse<T>>(result);
        }

        /// <summary>
        /// 数据模型：获取一条数据记录
        /// </summary>
        public Task<T> ModelsGet<T>(ModelsReqParams input)
        {
            return Models.Instance.Get<T>(input);
        }
        /// <summary>
        /// 数据模型：获取多条数据记录
        /// </summary>
        public Task<T> ModelsList<T>(ModelsReqParams input)
        {
            return Models.Instance.List<T>(input);
        }
        /// <summary>
        /// 数据模型：用于创建单条数据
        /// </summary>
        public Task<T> ModelsCreate<T>(ModelsReqParams input)
        {
            return Models.Instance.Create<T>(input);
        }
        /// <summary>
        /// 数据模型：用于创建多条数据
        /// </summary>
        public Task<T> ModelsCreateMany<T>(ModelsReqParams input)
        {
            return Models.Instance.CreateMany<T>(input);
        }
        /// <summary>
        /// 数据模型：用于更新单条数据
        /// </summary>
        public Task<T> ModelsUpdate<T>(ModelsReqParams input)
        {
            return Models.Instance.Update<T>(input);
        }
        /// <summary>
        /// 数据模型：用于更新多条数据
        /// </summary>
        public Task<T> ModelsUpdateMany<T>(ModelsReqParams input)
        {
            return Models.Instance.UpdateMany<T>(input);
        }
        /// <summary>
        /// 数据模型：用于删除单条数据
        /// </summary>
        public Task<T> ModelsDelete<T>(ModelsReqParams input)
        {
            return Models.Instance.Delete<T>(input);
        }
        /// <summary>
        /// 数据模型：用于删除多条数据
        /// </summary>
        public Task<T> ModelsDeleteMany<T>(ModelsReqParams input)
        {
            return Models.Instance.DeleteMany<T>(input);
        }

        private class App : IApp
        {
            private static readonly Lazy<App> _instance = new Lazy<App>(() => new());
            public static App Instance => _instance.Value;
            private App() { }

            IModels IApp.Models { get => Models.Instance; }

            public IDatabase Database()
            {
                return new Database();
            }
        }

        #region 数据模型

        private class Models : IModels
        {
            private static readonly Lazy<Models> _instance = new Lazy<Models>(() => new());
            public static Models Instance => _instance.Value;
            private Models() { }

            public Task<T> Get<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "get");
            }
            public Task<T> List<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "list");
            }
            public Task<T> Create<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "create");
            }
            public Task<T> CreateMany<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "createMany");
            }
            public Task<T> Update<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "update");
            }
            public Task<T> UpdateMany<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "updateMany");
            }
            public Task<T> Delete<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "delete");
            }
            public Task<T> DeleteMany<T>(ModelsReqParams input)
            {
                return ModelAPI<T>(input, "deleteMany");
            }

            private async Task<T> ModelAPI<T>(ModelsReqParams input, string apiName)
            {
                var realInput = new Dictionary<string, object>
                {
                    ["modelName"] = input.modelName,
                    ["options"] = JsonConvert.SerializeObject(input.options)
                };

                (string, TaskCompletionSource<string>) asyncTask = Internal.GetAsyncTask();

                Internal.Models_API(asyncTask.Item1, apiName, Internal.ParseInputParams(realInput));

                string result = await asyncTask.Item2.Task;
                return Internal.ParseOutputResult<T>(result);
            }
        }

        #endregion

        #region 云数据库

        private class Database : IDatabase
        {
            public ICommand Command => new CommandHandler();
            public ICollection Collection(string name) => new CollectionHandler(name);

            private static async Task<T> CommonHandler<T>(DatabaseAPIParam param)
            {
                (string, TaskCompletionSource<string>) asyncTask = Internal.GetAsyncTask();

                Internal.Database_API(asyncTask.Item1, JsonConvert.SerializeObject(param));

                string result = await asyncTask.Item2.Task;
                return Internal.ParseOutputResult<T>(result);
            }

            public class CollectionHandler : ICollection
            {
                private readonly string collectionName;
                public CollectionHandler(string name)
                {
                    collectionName = name;
                }

                public IDocument Doc(string id) => new DocumentHandler(collectionName, id);
                public Task<CollectionCreateRes> Add(object data)
                {
                    return CommonHandler<CollectionCreateRes>(new DatabaseAPIParam()
                    {
                        collectionName = collectionName,
                        chainList = new[] {
                            new ChainItem() {
                                method = "add",
                                optionsStr = JsonConvert.SerializeObject(data)
                            }
                        }
                    });
                }
                public IQuery Where(object filter) => new QueryHandler(collectionName, filter);
                public void Get() { /* 获取集合数据实现 */ }

            }

            public class DocumentHandler : IDocument
            {
                private string collectionName;
                private string documentId;
                public DocumentHandler(string collectionName, string id)
                {
                    this.collectionName = collectionName;
                    this.documentId = id;
                }

                public void Get() { /* 获取文档数据实现 */ }
                public void Update(object data) { /* 更新文档数据实现 */ }
                public void Set(object data) { /* 设置文档数据实现 */ }
                public void Remove() { /* 删除文档实现 */ }
            }

            public class QueryHandler : IQuery
            {
                private string collectionName;
                private object filter;
                public QueryHandler(string collectionName, object filter)
                {
                    this.collectionName = collectionName;
                    this.filter = filter;
                }

                public Task<T> Get<T>()
                {
                    return CommonHandler<T>(new DatabaseAPIParam()
                    {
                        collectionName = collectionName,
                        chainList = new[] {
                            new ChainItem() {
                                method = "where",
                                optionsStr = JsonConvert.SerializeObject(filter)
                            },
                            new ChainItem() {
                                method = "get",
                                optionsStr = ""
                            }
                        }
                    });
                }
                public void Update(object data) { /* 更新查询结果实现 */ }
                public void Remove() { /* 删除查询结果实现 */ }

                public IWatchObj Watch<T>(WatchParams<T> param)
                {

                    string uuid = "watch_" + Guid.NewGuid().ToString();
                    WatchObj cls = new(uuid, (string data) => param.OnChange(JsonConvert.DeserializeObject<WatchChangeData<T>>(data)), (string data) => param.OnError(JsonConvert.DeserializeObject<string>(data)));
                    Internal.watchDictionary.Add(uuid, cls);
                    Internal.Database_API(uuid, JsonConvert.SerializeObject(new DatabaseAPIParam()
                    {
                        collectionName = collectionName,
                        chainList = new[] {
                              new ChainItem()
                              {
                                method = "where",
                                optionsStr = JsonConvert.SerializeObject(filter)
                              },
                              new ChainItem()
                              {
                                method = "watch",
                                optionsStr = JsonConvert.SerializeObject(new Dictionary<string, string>{
                                    ["action"] = "open"
                                })
                              }
                        }
                    }));
                    return cls;
                }
            }

            public class CommandHandler : ICommand
            {
                public object GreaterThan(string fieldName, int value)
                {
                    return new
                    {
                    };
                }
            }

        }

        private class ChainItem
        {
            public string method { get; set; }
            public string optionsStr { get; set; }
        }
        private class DatabaseAPIParam
        {
            public string collectionName { get; set; }
            public ChainItem[] chainList { get; set; }
        }

        private class WatchObj : IWatchObj
        {
            readonly string callbackId;

            private event OnWatchHandler<string> OnChange;
            private event OnWatchHandler<string> OnError;

            public WatchObj(string callbackIdInput, OnWatchHandler<string> changeCallback, OnWatchHandler<string> errorCallback)
            {
                callbackId = callbackIdInput;
                OnChange += changeCallback;
                OnError += errorCallback;
            }

            public void Close()
            {
                Internal.Database_API(callbackId, JsonConvert.SerializeObject(new DatabaseAPIParam()
                {
                    chainList = new[] {
                              new ChainItem() {
                                method = "watch",
                                optionsStr = JsonConvert.SerializeObject(new Dictionary<string, string>{
                                    ["action"] = "close"
                                })
                            },
                        }
                }));
                Internal.watchDictionary.Remove(callbackId);
            }

            public void PerformChangeAction(string msg)
            {
                OnChange?.Invoke(msg);
            }

            public void PerformErrorAction(string err)
            {
                OnError?.Invoke(err);
            }
        }

        #endregion

        #endregion

        #region 内部方法

        private class Internal
        {

            [DllImport("__Internal")]
            public static extern void CloudInit(string callbackId, string input);

            [DllImport("__Internal")]
            public static extern void Auth_SignInAnonymously(string callbackId);

            [DllImport("__Internal")]
            public static extern string GetUserUUID();

            [DllImport("__Internal")]
            public static extern void CallFunction(string callbackId, string input);

            [DllImport("__Internal")]
            public static extern void Models_API(string callbackId, string apiName, string input);

            [DllImport("__Internal")]
            public static extern void Database_API(string callbackId, string input);

            public static string ParseInputParams<T>(T InputParams)
            {
                return JsonConvert.SerializeObject(InputParams);
            }

            public static T ParseOutputResult<T>(string output)
            {
                return JsonConvert.DeserializeObject<T>(output);
            }

            public static (string, TaskCompletionSource<string>) GetAsyncTask()
            {
                string uuid = Guid.NewGuid().ToString();
                TaskCompletionSource<string> tcs = new();
                tcsDictionary.Add(uuid, tcs);
                return (uuid, tcs);
            }

            public static readonly Dictionary<string, TaskCompletionSource<string>> tcsDictionary = new();

            public static readonly Dictionary<string, WatchObj> watchDictionary = new();

            public static string CONST_GAME_OBJECT_NAME = "TCBSDK";

            // =================== Private：仅仅为了可以在 jslib 内部互相调用 =================== //

            [DllImport("__Internal")]
            private static extern void GetCloudbaseWXCloudClientSdkInstance();

            [DllImport("__Internal")]
            private static extern void GetCloudbaseJSSdkScriptInstance();

            [DllImport("__Internal")]
            private static extern void CloudbaseWXCloudClientSdkScript();

#if !WEIXINMINIGAME
            [DllImport("__Internal")]
            private static extern void CloudbaseJSSdkScript();
#endif

            [DllImport("__Internal")]
            private static extern string Platfrom();

            [DllImport("__Internal")]
            private static extern void Constants();

            [DllImport("__Internal")]
            private static extern void GetGlobalData();

            [DllImport("__Internal")]
            private static extern void SetGlobalData();

            [DllImport("__Internal")]
            private static extern void Utils();
        }

        #endregion

        private class AsyncResponse<T>
        {
            public string callbackId { get; set; }
            public T result { get; set; }
        }

        /**
        * JavaScript 异步方法回调调用的方法
        * 注意：必须是这个名字，配合 tcbsdk.jslib，请不要修改
        */
        public void OnAsyncFnCompleted(string result)
        {
            AsyncResponse<string> res = Internal.ParseOutputResult<AsyncResponse<string>>(result);

            if (res.callbackId.StartsWith("watch_"))
            {
                var resultData = Internal.ParseOutputResult<Dictionary<string, object>>(res.result);
                if (resultData.ContainsKey("err"))
                {
                    Internal.watchDictionary[res.callbackId].PerformErrorAction(resultData["err"] as string);
                }
                else
                {
                    Internal.watchDictionary[res.callbackId].PerformChangeAction(JsonConvert.SerializeObject(resultData["data"]));
                }

            }
            else
            {
                Internal.tcsDictionary[res.callbackId].SetResult(res.result);
                // FIXME: 这一句是为了让整个回调能够正常执行，但原因不明，也很奇怪，后面再深入了解
                Task.Factory.StartNew(() => { });
                Internal.tcsDictionary.Remove(res.callbackId);
            }
        }
    }

    #region 类型和接口定义

    public interface IApp
    {
        IModels Models { get; }

        IDatabase Database();
    }

    public interface IModels
    {
        Task<T> Get<T>(ModelsReqParams input);
        Task<T> List<T>(ModelsReqParams input);
        Task<T> Create<T>(ModelsReqParams input);
        Task<T> CreateMany<T>(ModelsReqParams input);
        Task<T> Update<T>(ModelsReqParams input);
        Task<T> UpdateMany<T>(ModelsReqParams input);
        Task<T> Delete<T>(ModelsReqParams input);
        Task<T> DeleteMany<T>(ModelsReqParams input);
    }

    /// <summary>
    /// 云开发初始参数
    /// </summary>
    public class CloudInitParams
    {
        public string env { get; set; }
    }

    /// <summary>
    /// 调用云函数参数
    /// </summary>
    public class CallFunctionParams
    {
        public string name { get; set; }
        public object data { get; set; }
    }

    /// <summary>
    /// 调用云函数响应
    /// </summary>
    public class CallFunctionResponse<T>
    {
        public string code { get; set; }
        public string message { get; set; }
        public T result { get; set; }
        public string requestId { get; set; }
    }

    /// <summary>
    /// 数据模型接口请求参数
    /// </summary>
    public class ModelsReqParams
    {
        public string modelName { get; set; }
        public Dictionary<string, object> options { get; set; }
    }

    /// <summary>
    /// 数据模型响应列表类型
    /// </summary>
    public class ModelsList<T>
    {
        public int total { get; set; }
        public List<T> records { get; set; }
    }

    /// <summary>
    /// 数据模型系统字段类型
    /// </summary>
    public class ModelsSystem
    {
        public string createBy { get; set; }
        public long createdAt { get; set; }
        public string owner { get; set; }
        public string updateBy { get; set; }
        public long updatedAt { get; set; }
        public string _id { get; set; }
        public string _openId { get; set; }
    }

    /// <summary>
    /// 创建数据模型响应数据类型
    /// </summary>
    public class ModelsCreateRes
    {
        public string id { get; set; }
    }

    /// <summary>
    /// 创建多条数据模型响应数据类型
    /// </summary>
    public class ModelsCreateManyRes
    {
        public string[] idList { get; set; }
    }

    /// <summary>
    /// 更新数据模型响应数据类型
    /// </summary>
    public class ModelsUpdateRes
    {
        public int count { get; set; }
    }

    /// <summary>
    /// 删除数据模型响应数据类型
    /// </summary>
    public class ModelsDeleteRes : ModelsUpdateRes { }

    public interface IDatabase
    {
        ICollection Collection(string name);
        ICommand Command { get; }
    }

    public interface ICollection
    {
        IDocument Doc(string id);
        Task<CollectionCreateRes> Add(object data);
        IQuery Where(object filter);
        void Get();
    }

    public interface IDocument
    {
        void Get();
        void Update(object data);
        void Set(object data);
        void Remove();
    }

    public interface IQuery
    {
        Task<T> Get<T>();
        void Update(object data);
        void Remove();

        IWatchObj Watch<T>(WatchParams<T> param);
    }

    public interface ICommand
    {
        object GreaterThan(string fieldName, int value);
    }

    public delegate void OnWatchHandler<T>(T data);
    public interface IWatchObj
    {
        void Close();
    }
    public class WatchParams<T>
    {
        public OnWatchHandler<WatchChangeData<T>> OnChange { get; set; }
        public OnWatchHandler<string> OnError { get; set; }
    }
    public class WatchChangeData<T>
    {
        public WatchChangeDataDocChange<T>[] docChanges { get; set; }
        public T[] docs { get; set; }
        public int id { get; set; }

        // init 为初始连接发送。undefined 为后续发送
        public string type { get; set; }
    }

    public class WatchChangeDataDocChange<T>
    {
        public string dataType { get; set; }
        public T doc { get; set; }
        public string docId { get; set; }
        public int id { get; set; }
        public string queueType { get; set; }
    }

    /// <summary>
    /// 云数据库集合创建单条数据响应数据类型
    /// </summary>
    public class CollectionCreateRes
    {
        public string id { get; set; }
    }

    #endregion
}


