using Assets.Modules.Utils.Scripts.Components;
using Modules.Initializer.Scripts.Core;
using Modules.Initializer.Scripts.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Initializer.Scripts.Examples
{
    public class Example : MonoBehaviour
    {
        [SerializeField] private Updater _updater;

        private void Start()
        {
            var tasks = new List<TaskBase>()
            {
                //new LogTask("01 Start", LogType.Error, 1),
                new PauseTask(_updater, 1, 1),
                //new LogTask("02", LogType.Error, 1),
                new PauseTask(_updater, 2, 2),
                //new LogTask("03", LogType.Error, 1),
                new PauseTask(_updater, 3, 3),
                //new LogTask("04", LogType.Error, 1),
                new PauseTask(_updater, 4, 4),
                //new LogTask("05", LogType.Error, 1),
                new PauseTask(_updater, 5, 5),
                //new LogTask("06 End", LogType.Error, 1),
            };

            var tasker = new InitializeTasker(tasks);
            tasker.OnProgressChange += (x, y) => UnityEngine.Debug.LogError($"{x}/{y}");
            tasker.Run(() => { },(_) => { });
        }
    }
}
