using Servant.Validation;

namespace Servant.RequestParams
{
    public class ListDirectoryRequest
    {
        [RootedPath]
        public string Path { get; set; }
    }
}
