using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GrasshopperClosestPointComparison
{
    public class RtreeClosestpoint : GH_Component
    {
        public RtreeClosestpoint() : base("RTreeCp", "RTreeCp", "RTreeCp", "TreeComparison", "Comparison")
        {

        }
        public override Guid ComponentGuid => new Guid("{CF3976F5-1A07-4AB2-8AF6-293B053640AC}");
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


            var result = RTree.Point3dKNeighbors(inputCloud, inputPts, count);
            var outPts = new GH_Structure<GH_Point>();
            var outIdx = new GH_Structure<GH_Integer>();

            int branchIdx = 0;
            foreach (var foundIdx in result)
            {
                var path = new GH_Path(branchIdx);
                outPts.AppendRange(foundIdx.Select(idx => new GH_Point(inputPts[idx])), path);
                //outIdx.AppendRange(foundIdx.Select(idx => new GH_Integer(idx)), path);
                branchIdx++;
            }

            da.SetDataTree(0, outPts);
            da.SetDataTree(1, outIdx);
        }
    }
}
