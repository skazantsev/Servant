using Servant.Validation;

namespace Servant.RequestParams
{
    public class GetFileRequest
    {
        [RootedPath]
        public string Path { get; set; }
    }
}
