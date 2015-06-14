using System.Collections.Generic;
using System.Linq;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;
using MovieOCD.Models;

namespace MovieOCD.Mappers
{
    public class EditorPickMapper
    {
        public List<EditorPickResponse> MapFromModelListToResponse(List<EditorPick> editorPicks)
        {
            return editorPicks.Select(MapFromModelToResponse).ToList();
        }

        public EditorPickResponse MapFromModelToResponse(EditorPick editorPick)
        {
            var editorpickResponse = new EditorPickResponse
                {
                    IMDBID = editorPick.IMDBID,
                    Title = editorPick.Title,
                    EditorName = editorPick.EditorName,
                    Year = editorPick.Year,
                    EditorPickID = editorPick.EditorPickID.ToString()
                };

            return editorpickResponse;
        }
        
        public EditorPick MapFromRequestToModel(EditorPickRequest editorPickRequest)
        {
            var editorPick = new EditorPick
                {
                    IMDBID = editorPickRequest.IMDBID,
                    EditorName = editorPickRequest.EditorName
                };

            return editorPick;
        }

    }
}
