using System.Collections.Generic;
using MovieOCD.Models;

namespace MovieOCD.DataAccess.Repositories
{
    public interface IEditorPickRepository
    {
        EditorPick Add(EditorPick editorPick);

        List<EditorPick> RetrieveAll(string userID);
    }
}
