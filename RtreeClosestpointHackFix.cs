using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GrasshopperClosestPointComparison
{
    public class RtreeClosestpointHackFix : GH_Component
    {
        public RtreeClosestpointHackFix() : base("RTreeCpHackFix", "RTreeCpHackFix", "RTreeCpHackFix", "TreeComparison", "Comparison")
        {

        }
        public override Guid ComponentGuid => new Guid("{657967B8-B6A0-483E-827B-023C89290D5B}");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points to search from", GH_ParamAccess.list);
            pManager.AddPointParameter("Cloud", "C", "Cloud to search through", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number", "N", "Amount of results to return", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Found closest points", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Integer", "I", "Found integers", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess da)
        {
            var inputPts = new List<Point3d>();
            var inputCloud = new List<Point3d>();
            var count = 1;

            if (!da.GetDataList("Points", inputPts) 
                || !da.GetDataList("Cloud", inputCloud)
                || !da.GetData("Number", ref count))
            {
                return;
            }

            var useCount = Math.Max(count, 5);
            var postProcess = count < useCount;

            var result = RTree.Point3dKNeighbors(inputCloud, inputPts, useCount);
            var outPts = new GH_Structure<GH_Point>();
            var outIdx = new GH_Structure<GH_Integer>();

            int branchIdx = 0;
            foreach (var foundIdx in result)
            {
                var path = new GH_Path(branchIdx);
                var pts = foundIdx.Select(idx => new GH_Point(inputCloud[idx]));

                if (postProcess)
                {
                    var localBranchIdx = branchIdx;
                    pts = foundIdx.Select(idx =>
                        new
                        {
                            Point = inputCloud[idx],
                            DistanceSquared = inputPts[idx].DistanceToSquared(inputCloud[localBranchIdx])
                        })
                        .OrderBy(pt => pt.DistanceSquared)
                        .Select(pt => new GH_Point(pt.Point))
                        .Take(count);
                }

                outPts.AppendRange(pts, path);
                branchIdx++;
            }

            da.SetDataTree(0, outPts);
            da.SetDataTree(1, outIdx);
        }
    }
}
