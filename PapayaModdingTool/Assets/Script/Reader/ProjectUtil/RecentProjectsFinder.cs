using System.Collections.Generic;
using System.IO;
using System.Linq;
using PapayaModdingTool.Assets.Script.Misc.Paths;

namespace PapayaModdingTool.Assets.Script.Reader.ProjectUtil
{
    public class RecentProjectsFinder
    {
        public static List<string> FindRecentProjects()
        {
            string searchPath = PredefinedPaths.ProjectsPath;
            if (!Directory.Exists(searchPath))
                Directory.CreateDirectory(searchPath);
            
            string[] topLevelFolders = Directory.GetDirectories(searchPath, "*", SearchOption.TopDirectoryOnly);
            return topLevelFolders.ToList();
        }
    }
}