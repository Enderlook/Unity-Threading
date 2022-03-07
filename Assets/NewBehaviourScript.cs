using Enderlook.Unity.Jobs;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        /*JobManager.Factory.StartNew(() => Debug.Log("A"));
        JobManager.Factory.StartNew(() => Debug.Log("B"));*/
        /*JobManager.Factory.StartNew(async () =>
        {
            Debug.Log("C");
            await Task.Yield();
            Debug.Log("D");
            await Task.Delay(1);
            Debug.Log("E");
            /*await JobManager.Factory.StartNew(() => Debug.Log("F"));
            Debug.Log("G");
        });*/
        /*JobManager.Factory.StartNew(() => Debug.Log("Z"));*/

        //AsyncHelpers.RunInJob(() => Debug.Log("A"));
        //AsyncHelpers.RunInJob(() => Debug.Log("B"));
        AsyncHelpers.RunInJob(async () =>
        {
            Debug.Log("C");
            await Task.Yield();
            Debug.Log("D");
            await Task.Delay(1);
            Debug.Log("E");
            await AsyncHelpers.RunInJob(async () => {
                Debug.Log("F");
                await Task.Yield();
                Debug.Log("G");
                await Task.Delay(1);
                Debug.Log("H");
            });
            Debug.Log("I");
        }).WatchCompletion().Complete();
        //AsyncHelpers.RunInJob(() => Debug.Log("Z"));
    }
}
