using System.Text.Json;

namespace BrickManager
{
    public class BrickDependencyResolver
    {
        private readonly string engineRootPath;
        private readonly Dictionary<string, BrickInfo> brickCache = new Dictionary<string, BrickInfo>(StringComparer.OrdinalIgnoreCase);

        public BrickDependencyResolver(string engineRootPath)
        {
            this.engineRootPath = engineRootPath;
        }

        /// <summary>
        /// Scan the engine root directory for all .brick files and parse them
        /// </summary>
        public List<BrickInfo> ScanAllBricks()
        {
            brickCache.Clear();
            var brickFiles = Directory.GetFiles(engineRootPath, "*.brick", SearchOption.AllDirectories);
            var results = new List<BrickInfo>();

            foreach (var filePath in brickFiles)
            {
                var brick = LoadBrickFile(filePath);
                if (brick != null)
                {
                    results.Add(brick);
                }
            }

            return results;
        }

        /// <summary>
        /// Load and parse a single .brick file. Results are cached by relative path.
        /// </summary>
        private BrickInfo LoadBrickFile(string absolutePath)
        {
            var relativePath = System.IO.Path.GetRelativePath(engineRootPath, absolutePath).Replace('\\', '/');

            if (brickCache.TryGetValue(relativePath, out var cached))
            {
                return cached;
            }

            try
            {
                var jsonContent = File.ReadAllText(absolutePath);
                var brick = JsonSerializer.Deserialize<BrickInfo>(jsonContent);
                if (brick != null)
                {
                    brick.AbsoluteFilePath = absolutePath;
                    brickCache[relativePath] = brick;
                    return brick;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse brick file: {absolutePath}, Error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Resolve all transitive dependencies for the given selected bricks.
        /// Returns the full set of bricks (selected + all their transitive dependencies).
        /// </summary>
        public List<BrickInfo> ResolveAllDependencies(IEnumerable<BrickInfo> selectedBricks)
        {
            var resolvedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var resolvedBricks = new List<BrickInfo>();

            foreach (var brick in selectedBricks)
            {
                ResolveDependenciesRecursive(brick, resolvedSet, resolvedBricks);
            }

            return resolvedBricks;
        }

        /// <summary>
        /// Recursively resolve dependencies for a single brick
        /// </summary>
        private void ResolveDependenciesRecursive(BrickInfo brick, HashSet<string> resolvedSet, List<BrickInfo> resolvedBricks)
        {
            var relativePath = System.IO.Path.GetRelativePath(engineRootPath, brick.AbsoluteFilePath).Replace('\\', '/');

            if (resolvedSet.Contains(relativePath))
            {
                return;
            }

            resolvedSet.Add(relativePath);
            resolvedBricks.Add(brick);

            foreach (var dependencyPath in brick.SharedProjects)
            {
                var absoluteDependencyPath = System.IO.Path.Combine(engineRootPath, dependencyPath.Replace('/', '\\'));

                if (!File.Exists(absoluteDependencyPath))
                {
                    Console.WriteLine($"Warning: Dependency not found: {dependencyPath} (referenced by {brick.Name})");
                    continue;
                }

                var dependencyBrick = LoadBrickFile(absoluteDependencyPath);
                if (dependencyBrick != null)
                {
                    ResolveDependenciesRecursive(dependencyBrick, resolvedSet, resolvedBricks);
                }
            }
        }
    }
}
