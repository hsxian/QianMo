using System;
using System.Collections.Generic;
using QianMo.Core.Models;

namespace QianMo.Core.Clustering
{
    public interface ICommonSubsequenceCluster<TGeoCode>
    {
        IEnumerable<TrajectoryWithCode<TGeoCode>> BuildClusterTree(IEnumerable<TrajectoryWithCode<TGeoCode>> trajectories, float scaleSimilar = 0.8f,
            float scaleBlood = 0.6f);

        void ForeachTree(IEnumerable<TrajectoryWithCode<TGeoCode>> tree, Action<TrajectoryWithCode<TGeoCode>> action);
    }
}