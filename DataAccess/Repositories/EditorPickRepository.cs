using System.Collections.Generic;
using System.Data;
using System.Linq;
using MovieOCD.DataAccess.Context;
using MovieOCD.Models;

namespace MovieOCD.DataAccess.Repositories
{
    public class EditorPickRepository : IEditorPickRepository
    {
        public EditorPick Add(EditorPick editorPick)
        {
            using (var movieOCDDB = new MovieOCDDB())
            {
                var hasExistingIMDBRecord = movieOCDDB.EditorPick.FirstOrDefault(x => x.IMDBID == editorPick.IMDBID && x.EditorName == editorPick.EditorName);

                if (hasExistingIMDBRecord != null)
                {
                    return editorPick;
                }

                movieOCDDB.EditorPick.Add(editorPick);
                movieOCDDB.Entry(editorPick).State = EntityState.Added;
                movieOCDDB.SaveChanges();
                return editorPick;
        }
        }

        public List<EditorPick> RetrieveAll(string imdbid)
        {
            using (var db = new MovieOCDDB())
            {
                var queryResults = from pick in db.EditorPick
                                   where (pick.IMDBID == imdbid)
                                   select pick;

                return queryResults.ToList();

            }
        }
    }
}
