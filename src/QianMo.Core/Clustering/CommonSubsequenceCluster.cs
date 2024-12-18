using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QianMo.Core.Models;

namespace QianMo.Core.Clustering
{
    public class CommonSubsequenceCluster<TGeoCode> : ICommonSubsequenceCluster<TGeoCode>
    {
        public IEnumerable<TrajectoryWithCode<TGeoCode>> BuildClusterTree(IEnumerable<TrajectoryWithCode<TGeoCode>> trajectories, float scaleSimilar = 0.8f,
            float scaleBlood = 0.6f)
        {
            var array = trajectories.OrderByDescending(t => t.GeoCodes.Count).ToArray();
            var length = array.Length;

            for (var i = 0; i < length; i++)
            {
                var ft = array[i];
                Parallel.For(i + 1, length, j =>
                {
                    var ct = array[j];
                    var uid = $"{i}_{j}";
                    if (ct.Level == 1
                    || ft.DoRectanglesIntersect(ct) == false
                    ) return;

                    var intersect = ft.GeoCodes.Intersect(ct.GeoCodes).ToList();
                    var intersectCount = (float)intersect.Count;
                    var rateIbf = intersectCount / ft.GeoCodes.Count;
                    var rateJaf = intersectCount / ct.GeoCodes.Count;

                    if (rateIbf >= scaleSimilar && rateJaf >= scaleSimilar)
                    {
                        ct.Level = 1;
                        ft.Siblings.Add(ct);
                        ct.Siblings.Add(ft);
                    }
                    else if (rateJaf > rateIbf && rateIbf >= scaleBlood)
                    {
                        ct.Level = 1;
                        ct.Parent = ft;
                        ft.Children.Add(ct);
                    }
                    else if (rateIbf > rateJaf && rateJaf >= scaleBlood)
                    {
                        ft.Level = 1;
                        ft.Parent = ct;
                        ct.Children.Add(ft);
                    }
                });
                Console.WriteLine($"BuildClusterTree:{i}/{length}");
            }

            var root = array.Where(t => t.Level == 0).ToList();

            SetClusterTreeLevelInfo(root);

            return root;
        }


        private void SetClusterTreeLevelInfo(IEnumerable<TrajectoryWithCode<TGeoCode>> tree, int level = 0, string levelTag = "")
        {
            Parallel.For(0, tree.Count(), i =>
            {
                var node = tree.ElementAt(i);

                node.Level = level;
                node.LevelTag = $"{levelTag}{i}";

                for (var j = 0; j < node.Siblings.Count; j++)
                {
                    var sib = node.Siblings.ElementAt(j);
                    sib.Level = level;
                    sib.LevelTag = $"{node.LevelTag}__sim_{j}";
                }

                SetClusterTreeLevelInfo(node.Children, level + 1, $"{node.LevelTag}_");
            });
        }

        public void ForeachTree(IEnumerable<TrajectoryWithCode<TGeoCode>> tree, Action<TrajectoryWithCode<TGeoCode>> action)
        {
            foreach (var node in tree)
            {
                action(node);
                ForeachTree(node.Children, action);
            }
        }
    }
}