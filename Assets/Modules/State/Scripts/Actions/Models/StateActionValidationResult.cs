namespace Modules.State.Scripts.Actions.Models
{
    public class StateActionValidationResult
    {
        public static readonly StateActionValidationResult Ok = new StateActionValidationResult(true, string.Empty, 0);

        public bool IsValid { get; }
        public string ErrorMessage { get; }
        public int ErrorCode { get; }

        public StateActionValidationResult(bool isValid, string errorMessage, int errorCode = 0)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage ?? string.Empty;
            ErrorCode = errorCode;
        }

        public static StateActionValidationResult Fail(string errorMessage, int errorCode = 1)
        {
            return new StateActionValidationResult(false, errorMessage, errorCode);
        }
    }
}
