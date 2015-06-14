using System.Collections.Generic;
using System.Web.Http;
using MovieOCD.BusinessLogic;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;

namespace MovieOCD.Controllers
{
    public class EditorPickController : ApiController
    {
        private readonly EditorPickManager _editorPickManager;

        public EditorPickController()
        {
            _editorPickManager = new EditorPickManager();
        }

        public List<EditorPickResponse> Get(string imdbid)
        {
            return _editorPickManager.CheckForPick(imdbid);
        }

        public EditorPickResponse Post(EditorPickRequest request)
        {
            return _editorPickManager.AddPick(request);
        }
    }
}
