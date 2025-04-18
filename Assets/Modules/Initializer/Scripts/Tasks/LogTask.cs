using UnityEngine;

namespace Modules.Initializer.Scripts.Tasks
{
    public class LogTask : TaskBase
    {
        private string _message;
        private LogType _type;

        public LogTask(string message, LogType type, int weight) : base(weight)
        {
            _message = message;
            _type = type;
        }

        public override void Run()
        {
            switch (_type)
            {
                case LogType.Error:
                    UnityEngine.Debug.LogError(_message);
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(_message);
                    break;

                default:
                    UnityEngine.Debug.Log(_message);
                    break;
            }

            Complete();
        }
    }
}
