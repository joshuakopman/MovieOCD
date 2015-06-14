using System.Collections.Generic;
using MovieOCD.DataAccess.Repositories;
using MovieOCD.Mappers;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;

namespace MovieOCD.BusinessLogic
{
    public class EditorPickManager
    {
        private readonly EditorPickMapper _mapper;
        private readonly IEditorPickRepository _editorpickrepository;

        public EditorPickManager()
        {
            _mapper = new EditorPickMapper();
            _editorpickrepository = new EditorPickRepository();
        }

        public EditorPickManager(IEditorPickRepository repoIOC)
        {
            _mapper = new EditorPickMapper();
            _editorpickrepository = repoIOC;
        }
       
        public List<EditorPickResponse> CheckForPick(string imdbid)
        {
            var epModel = _editorpickrepository.RetrieveAll(imdbid);

            return _mapper.MapFromModelListToResponse(epModel);
        }

        public EditorPickResponse AddPick(EditorPickRequest editorPickRequest)
        {
            var epModel = _mapper.MapFromRequestToModel(editorPickRequest);

            var updatedModel = _editorpickrepository.Add(epModel);

            return _mapper.MapFromModelToResponse(updatedModel);
        }
    }
}
