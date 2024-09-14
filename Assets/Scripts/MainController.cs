using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo;
using Newtonsoft.Json;
using TencentCloud.CloudBase;
using UnityEngine;

public class MainController : MonoBehaviour
{

    IApp app;

    TencentCloud.CloudBase.IWatchObj watchObj;

    async void Start()
    {
        // DemoSDK.Instance.Hello();
        // DemoSDK.Instance.HelloWithInput(new HelloWithInputParams() { name = "daniel666" });
        // HelloWithReturnResult output = DemoSDK.Instance.HelloWithReturn();
        // Debug.Log(">>>>>>" + output);
        // Debug.Log(">>>>>>" + output.name);
        // DemoSDK.Instance.HelloCallOtherFn();
        // HelloWithReturnResult result1 = await DemoSDK.Instance.HelloAsyncFn(new HelloWithInputParams() { name = "daniel666" });
        // Debug.Log("done HelloAsyncFn 1: " + result1.name);
        // HelloWithReturnResult result2 = await DemoSDK.Instance.HelloAsyncFn(new HelloWithInputParams() { name = "sarah666" });
        // Debug.Log("done HelloAsyncFn 2: " + result2.name);

        // var watchObj = DemoSDK.Instance.HelloWatchFn().Watch<string>((msg) =>
        // {
        //     Debug.Log($"Unity Receive message: {msg}");
        // });

        // await Wait(3);
        // Debug.Log(">>> Wait 3 second done");
        // watchObj.Close();

        // DemoSDK.Instance.HelloAsyncFn(new HelloWithInputParams() { name = "daniel666" }).ContinueWith(task =>
        // {
        //     Debug.Log("done HelloAsyncFn 1: " + task.Result.name);
        // }, TaskScheduler.FromCurrentSynchronizationContext());
        // DemoSDK.Instance.HelloAsyncFn(new HelloWithInputParams() { name = "sarah666" }).ContinueWith(task =>
        // {
        //     Debug.Log("done HelloAsyncFn 2: " + task.Result.name);
        // }, TaskScheduler.FromCurrentSynchronizationContext());

#if WEIXINMINIGAME
                // 1. init
                app = await TCBSDK.Instance.Init(new CloudInitParams() { env = TestEnv.WX_ENV });
                Debug.Log(">>> userUUID:" + TCBSDK.Instance.GetUserUUID());

                // 2. 调用 API。比如 TCBSDK.Instance.ModelsGet 或 app.Models.Get
                // TestModelGet("9bf3df1366d84eb209a2c0a2099c5685");
                // TestModelList();
                // (string, string) createId = await TestModelCreate();
                // await TestModelUpdate(createId.Item1);
                // await TestModelDelete(createId.Item1, createId.Item2);
                // await TestCallFunction();
                await TestDatabase();
#else
        // 1. init
        app = await TCBSDK.Instance.Init(new CloudInitParams() { env = TestEnv.WEB_ENV });

        // 2. auth
        await TCBSDK.Instance.Auth_SignInAnonymously();
        Debug.Log(">>> userUUID:" + TCBSDK.Instance.GetUserUUID());

        // 3. 调用 API。比如 TCBSDK.Instance.ModelsGet 或 app.Models.Get
        // TestModelGet("983e93c466d8418c008f7f88303e5006");
        // TestModelList();
        // (string, string) createId = await TestModelCreate();
        // await TestModelUpdate(createId.Item1);
        // await TestModelDelete(createId.Item1, createId.Item2);
        // await TestCallFunction();
        await TestDatabase();
#endif

    }

    public async Task Wait(float seconds)
    {
        if (seconds <= 0)
        {
            await Task.CompletedTask;
        }
        else
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(WaitForSecondsCoroutine(seconds, tcs));
            await tcs.Task;
        }
    }

    private static IEnumerator WaitForSecondsCoroutine(float seconds, TaskCompletionSource<bool> tcs)
    {
        yield return new WaitForSeconds(seconds);
        tcs.SetResult(true);
    }

    async void TestModelGet(string id)
    {
        var options = new Dictionary<string, object>
        {
            ["filter"] = new Dictionary<string, object>
            {
                ["where"] = new Dictionary<string, object>
                {
                    ["$and"] = new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object>
                            {
                                ["_id"] = new Dictionary<string, string>
                                {
                                    ["$eq"] = id
                                }
                            }
                        }
                }
            }
        };
        ModelHello hello1 = await TCBSDK.Instance.ModelsGet<ModelHello>(new ModelsReqParams() { modelName = "hello", options = options });
        ModelHello hello2 = await app.Models.Get<ModelHello>(new ModelsReqParams() { modelName = "hello", options = options });
        Debug.Log($"ModelsGet: id: {hello1._id} data: {JsonConvert.SerializeObject(hello1)}");
        Debug.Log($"app.Models.Get: id: {hello2._id} data: {JsonConvert.SerializeObject(hello1)}");
    }

    async void TestModelList()
    {
        var options = new Dictionary<string, object>
        {
            ["filter"] = new Dictionary<string, object>
            {
                ["where"] = new Dictionary<string, object>()
            },
            ["pageSize"] = 2,
            ["pageNumber"] = 1,
            ["getCount"] = true
        };
        ModelsList<ModelHello> hello1 = await TCBSDK.Instance.ModelsList<ModelsList<ModelHello>>(new ModelsReqParams() { modelName = "hello", options = options });
        ModelsList<ModelHello> hello2 = await app.Models.List<ModelsList<ModelHello>>(new ModelsReqParams() { modelName = "hello", options = options });
        Debug.Log($"ModelsList: total: {hello1.total} records: {JsonConvert.SerializeObject(hello1.records)}");
        Debug.Log($"app.Models.List: total: {hello2.total} records: {JsonConvert.SerializeObject(hello1.records)}");
    }

    async Task<(string, string)> TestModelCreate()
    {
        var options = new Dictionary<string, object>
        {
            ["data"] = new Dictionary<string, string>
            {
                ["name"] = "Lynn"
            }
        };
        ModelsCreateRes hello1 = await TCBSDK.Instance.ModelsCreate<ModelsCreateRes>(new ModelsReqParams() { modelName = "hello", options = options });
        ModelsCreateRes hello2 = await app.Models.Create<ModelsCreateRes>(new ModelsReqParams() { modelName = "hello", options = options });
        Debug.Log($"ModelsCreate: id: {hello1.id}");
        Debug.Log($"app.Models.Create: id: {hello2.id}");

        return (hello1.id, hello2.id);
    }

    async Task TestModelUpdate(string id)
    {
        var options = new Dictionary<string, object>
        {
            ["data"] = new Dictionary<string, object>
            {
                ["name"] = "daniel_" + Guid.NewGuid().ToString()
            },
            ["filter"] = new Dictionary<string, object>
            {
                ["where"] = new Dictionary<string, object>
                {
                    ["$and"] = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            ["_id"] = new Dictionary<string, string>
                            {
                                ["$eq"] = id
                            }
                        }
                    }
                }
            }
        };
        var hello1 = await TCBSDK.Instance.ModelsUpdate<ModelsUpdateRes>(new ModelsReqParams() { modelName = "hello", options = options });
        var hello2 = await app.Models.Update<ModelsUpdateRes>(new ModelsReqParams() { modelName = "hello", options = options });
        Debug.Log($"ModelsUpdate: id: {hello1.count}");
        Debug.Log($"app.Models.Update: id: {hello2.count}");
    }

    async Task TestModelDelete(string id1, string id2)
    {
        var options1 = new Dictionary<string, object>
        {
            ["filter"] = new Dictionary<string, object>
            {
                ["where"] = new Dictionary<string, object>
                {
                    ["$and"] = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            ["_id"] = new Dictionary<string, string>
                            {
                                ["$eq"] = id1
                            }
                        }
                    }
                }
            }
        };
        var hello1 = await TCBSDK.Instance.ModelsDelete<ModelsDeleteRes>(new ModelsReqParams() { modelName = "hello", options = options1 });
        Debug.Log($"ModelsDelete: id: {hello1.count}");

        var options2 = new Dictionary<string, object>
        {
            ["filter"] = new Dictionary<string, object>
            {
                ["where"] = new Dictionary<string, object>
                {
                    ["$and"] = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            ["_id"] = new Dictionary<string, string>
                            {
                                ["$eq"] = id2
                            }
                        }
                    }
                }
            }
        };
        var hello2 = await app.Models.Delete<ModelsDeleteRes>(new ModelsReqParams() { modelName = "hello", options = options2 });
        Debug.Log($"app.Models.Delete: id: {hello2.count}");
    }

    async Task TestCallFunction()
    {
        var data = new Dictionary<string, object>
        {
            ["modelName"] = "hello",
            ["apiName"] = "list",
            ["options"] = new Dictionary<string, object>
            {
                ["filter"] = new Dictionary<string, object>
                {
                    ["where"] = new Dictionary<string, object>()
                },
                ["pageSize"] = 10,
                ["pageNumber"] = 1,
                ["getCount"] = true
            }
        };
        var res = await TCBSDK.Instance.CallFunction<ModelsList<ModelHello>>(new CallFunctionParams() { name = "modelsAPI", data = data });
        Debug.Log($"{res.code} | {res.requestId} | {res.message} | {res.result.records.Count}");
    }

    async Task TestDatabase()
    {
        var database = app.Database();

        var addRes = await database.Collection("hello").Add(new Dictionary<string, object>
        {
            ["name"] = "Thinking in Java"
        });
        Debug.Log($"addRes: {addRes.id}");

        var whereGetRes = await database.Collection("hello").Where(new Dictionary<string, object>
        {
        }).Get<ModelHello[]>();
        Debug.Log($"whereGetRes: {whereGetRes[0].name}");

        watchObj = database.Collection("hello").Where(new Dictionary<string, object>
        {
        }).Watch(new WatchParams<ModelHello>()
        {
            OnChange = (WatchChangeData<ModelHello> data) =>
            {
                if (data.type == "init")
                {
                    Debug.Log($"watch change: {JsonConvert.SerializeObject(data.docChanges)}");
                }
            },
            OnError = (string err) =>
            {
                Debug.Log($"watch err: {err}");
            }
        });

        await Task.FromResult("");
    }

    public void HandleCloseWatch() 
    {
        watchObj.Close();
    }
}



class ModelHello : ModelsSystem
{
    public string name { get; set; }
}
