// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Tomori.Framework.Extensions.ObjectExtensions;

namespace Tomori.Framework.Development;

/// <summary>
/// Utility class for debugging purposes.
/// </summary>
public class DebugUtils
{
    public static bool IsNUnitRunning => is_nunit_running.Value;

    private static readonly Lazy<bool> is_nunit_running = new Lazy<bool>(() =>
        {
#pragma warning disable RS0030
            var entry = Assembly.GetEntryAssembly();
#pragma warning restore RS0030

            string? assemblyName = entry?.GetName().Name;

            // when running under nunit + netcore, entry assembly becomes nunit itself (testhost, Version=15.0.0.0), which isn't what we want.
            // when running under nunit + Rider > 2020.2 EAP6, entry assembly becomes ReSharperTestRunner[32|64], which isn't what we want.
            bool entryIsKnownTestAssembly = entry != null && (assemblyName!.Contains("testhost") || assemblyName.Contains("ReSharperTestRunner"));

            // null assembly can indicate nunit, but it can also indicate native code (e.g. android).
            // to distinguish nunit runs from android launches, check the class name of the current test.
            // if no actual test is running, nunit will make up an ad-hoc test context, which we can match on
            // to eliminate such false positives.
            bool nullEntryWithActualTestContext = entry == null && TestContext.CurrentContext.Test.ClassName != typeof(TestExecutionContext.AdhocContext).FullName;

            return entryIsKnownTestAssembly || nullEntryWithActualTestContext;
        }
    );

    internal static Assembly NUnitTestAssembly => nunit_test_assembly.Value;

    private static readonly Lazy<Assembly> nunit_test_assembly = new Lazy<Assembly>(() =>
        {
            Debug.Assert(IsNUnitRunning);

            string testName = TestContext.CurrentContext.Test.ClassName.AsNonNull();
            return AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.GetType(testName) != null);
        }
    );

    public static bool IsDebugBuild => is_debug_build.Value;

    private static readonly Lazy<bool> is_debug_build = new Lazy<bool>(() =>
        isDebugAssembly(typeof(DebugUtils).Assembly) || isDebugAssembly(RuntimeInfo.EntryAssembly)
    );

    // https://stackoverflow.com/a/2186634
    private static bool isDebugAssembly(Assembly? assembly) => assembly?.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled) ?? false;
}
