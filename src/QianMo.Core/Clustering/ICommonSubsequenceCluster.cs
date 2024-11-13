using System;
using System.Collections.Generic;
using QianMo.Core.Models;

namespace QianMo.Core.Clustering
{
    public interface ICommonSubsequenceCluster<TGeoCode>
    {
        IEnumerable<Trajectory<TGeoCode>> BuildClusterTree(IEnumerable<Trajectory<TGeoCode>> trajectories, float scaleSimilar = 0.8f,
            float scaleBlood = 0.6f);

        void ForeachTree(IEnumerable<Trajectory<TGeoCode>> tree, Action<Trajectory<TGeoCode>> action);
    }
}