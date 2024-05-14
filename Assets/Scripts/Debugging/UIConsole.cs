using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Debugging
{
    public class UIConsole : MonoBehaviour, ILogHandler
    {
        [SerializeField] private Text consoleBody;
        [SerializeField] private Color logColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color errorColor = new(.75f, .25f, .25f);
        [SerializeField] private Color assertColor = Color.magenta;
        [SerializeField] private Color exceptionColor = Color.red;

        [SerializeField] private bool shouldDebugLogTypes = false;
        private ILogHandler _unityLoggerLOGHandler;

        private void OnEnable()
        {
            if (!consoleBody)
            {
                Debug.LogError($"{name} {nameof(consoleBody)} is null!");
                return;
            }
            _unityLoggerLOGHandler = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = this;
#if UNITY_EDITOR
            if (shouldDebugLogTypes)
            {
                Debug.Log($"log");
                Debug.LogWarning($"warning");
                Debug.LogError($"error");
                Debug.LogException(new Exception("exception"));
                Debug.LogAssertion($"assertion");
            }
#endif
        }
        
        [HideInCallstack]
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            _unityLoggerLOGHandler.LogFormat(logType, context, format, args);
            LogFormatInternal(logType, format, args);
        }

        [HideInCallstack]
        private void LogFormatInternal(LogType logType, string format, object[] args)
        {
            var formattedLog = string.Format(format, args);
            consoleBody.text += logType switch
                                {
                                    LogType.Error =>
                                        $"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>{formattedLog}</color>",
                                    LogType.Assert =>
                                        $"<color=#{ColorUtility.ToHtmlStringRGB(assertColor)}>{formattedLog}</color>",
                                    LogType.Warning =>
                                        $"<color=#{ColorUtility.ToHtmlStringRGB(warningColor)}>{formattedLog}</color>",
                                    LogType.Log =>
                                        $"<color=#{ColorUtility.ToHtmlStringRGB(logColor)}>{formattedLog}</color>",
                                    LogType.Exception =>
                                        $"<color=#{ColorUtility.ToHtmlStringRGB(exceptionColor)}>{formattedLog}</color>",
                                    _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
                                };
            consoleBody.text += '\n';
        }

        [HideInCallstack]
        public void LogException(Exception exception, Object context)
        {
            _unityLoggerLOGHandler.LogException(exception, context);
            LogFormatInternal(LogType.Exception, "{0}\n{1}", new object[]{exception.Message, exception.StackTrace});
        }
    }
}
