using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QianMo.Core.Models;

namespace QianMo.Core.Cluster
{
    public class CommonSubsequenceCluster : ICommonSubsequenceCluster
    {
        public IEnumerable<Trajectory> BuildClusterTree(IEnumerable<Trajectory> trajectories, float scaleSimilar = 0.8f,
            float scaleBlood = 0.6f)
        {
            var array = trajectories.OrderByDescending(t => t.GeoCodes.Count).ToArray();

            for (var ibf = 0; ibf < array.Length; ibf++)
            {
                Parallel.For(ibf + 1, array.Length, jaf =>
                {
                    if (array[jaf].Level == 1) return;

                    var intersect = array[ibf].GeoCodes.Intersect(array[jaf].GeoCodes).ToList();
                    var intersectCount = (float) intersect.Count;
                    var rateIbf = intersectCount / array[ibf].GeoCodes.Count;
                    var rateJaf = intersectCount / array[jaf].GeoCodes.Count;

                    if (rateIbf >= scaleSimilar && rateJaf >= scaleSimilar)
                    {
                        array[jaf].Level = 1;
                        array[ibf].Siblings.Add(array[jaf]);
                        array[jaf].Siblings.Add(array[ibf]);
                    }
                    else if (rateJaf > rateIbf && rateIbf >= scaleBlood)
                    {
                        array[jaf].Level = 1;
                        array[jaf].Parent = array[ibf];
                        array[ibf].Children.Add(array[jaf]);
                    }
                    else if (rateIbf > rateJaf && rateJaf >= scaleBlood)
                    {
                        array[ibf].Level = 1;
                        array[ibf].Parent = array[jaf];
                        array[jaf].Children.Add(array[ibf]);
                    }
                });
            }

            var root = array.Where(t => t.Level == 0 ).ToList();

            SetClusterTreeLevelInfo(root);

            return root;
        }


        private void SetClusterTreeLevelInfo(IEnumerable<Trajectory> tree, int level = 0, string levelTag = "")
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

        public void ForeachTree(IEnumerable<Trajectory> tree, Action<Trajectory> action)
        {
            foreach (var node in tree)
            {
                action(node);
                ForeachTree(node.Children, action);
            }
        }
    }
}