using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeXl
{
    namespace Global
    {
        internal static class Resources
        {
            public static string MainControllerKey = "GlobalFaultTreeModel";
            public static FaultTreeModel MainController
            { get => System.Windows.Application.Current.FindResource(MainControllerKey) as FaultTreeModel; }
        }

        internal static class NodeConstants
        {
            // Node Constants
            public const string NODETYPE = "Node";
            public const string NAME = "Node 1";
            public const string DESCRIPTION = "Test Node Default";
            public const decimal PTI = 8760;
            public const decimal LAMBDA = 1E-06m;
            public const double BETA = 0.0;
            public const decimal PERFECT_PTE = 1.0M;
            public const decimal PTE = 1.0M;
            public const decimal LIFETIME = 87600M;
            public const decimal TOTAL_FAILURE_RATE = 1.0E-5M;
            public const bool IS_TYPE_A = true;
        }

        internal static class NodeUtils
        {
            public static string NextNodeNameCustom(string prefix)
                => $"{prefix} {Resources.MainController.NextNodeName(prefix)+1}"; 

            public static string NextNodeName
            {
                get => NextNodeNameCustom("Node");
            }
            public static void ReDrawRootNode()
                    => Resources.MainController.ReDrawRootNode();
        }
        internal static class ORConstants
        {
            public const string DEFAULT_OR_NODETYPE = "OR";
        }
        public static class ANDConstants
        {
            // AND Constants
            public const string DEFAULT_AND_NODETYPE = "AND";
            public const string DEFAULT_AND_NAME = "AND1";
            public const string DEFAULT_AND_DESCRIPTION = "Test AND Description";
            public const double DEFAULT_AND_BETA = 10.0;
        }
    }
}
