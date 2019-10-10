using System;
using System.Collections.Generic;
using QianMo.Core.Models;

namespace QianMo.Core.Cluster
{
    public interface ICommonSubsequenceCluster
    {
        IEnumerable<Trajectory> BuildClusterTree(IEnumerable<Trajectory> trajectories, float scaleSimilar = 0.8f,
            float scaleBlood = 0.6f);

        void ForeachTree(IEnumerable<Trajectory> tree, Action<Trajectory> action);
    }
}