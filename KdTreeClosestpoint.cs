using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GrasshopperClosestPointComparison
{
    public class KdTreeClosestpoint : GH_Component
    {
        public KdTreeClosestpoint() : base("KdTreeCp", "KdTreeCp", "KdTreeCp", "TreeComparison", "Comparison")
        {

        }
        public override Guid ComponentGuid => new Guid("{147E3DB8-0711-4C42-AE33-0B3B3FB9B5F0}");
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

            var kdtree = KDTree.FromData<int>(inputPts.Select(pt => new[] {pt.X, pt.Y, pt.Z}).ToArray());

            var outPts = new GH_Structure<GH_Point>();

            int branchIdx = 0;
            foreach (var searchPt in inputPts)
            {
                var path = new GH_Path(branchIdx);
                var searchPtArr = new[] {searchPt.X, searchPt.Y, searchPt.Z};
                var neighbours = kdtree.Nearest(searchPtArr, count);
                var pts = neighbours.Select(n =>
                {
                    var pos = n.Node.Position;
                    return new GH_Point(new Point3d(pos[0], pos[1], pos[2]));
                });

                outPts.AppendRange(pts, path);
                branchIdx++;
            }

            da.SetDataTree(0, outPts);
        }
    }
}
