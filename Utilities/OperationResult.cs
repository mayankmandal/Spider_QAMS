namespace Spider_QAMS.Utilities
{
    public class OperationResult
    {
        public bool Succeeded { get; private set; }
        public List<string> Errors { get; private set;}

        private OperationResult(bool succeeded, List<string> errors = null)
        {
            Succeeded = succeeded;
            Errors = errors ?? new List<string>();
        }
        public static OperationResult Success()
        {
            return new OperationResult(true);
        }
        public static OperationResult Failure(params string[] errors)
        {
            return new OperationResult(false, errors.ToList());
        }
    }
}
