// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// This copy of Ice is licensed to you under the terms described in the
// ICE_LICENSE file included in this distribution.
//
// **********************************************************************
//
// Ice version 3.7.1
//
// <auto-generated>
//
// Generated from file `SampleAccount.ice'
//
// Warning: do not edit this file.
//
// </auto-generated>
//


using _System = global::System;

#pragma warning disable 1591

namespace IceCompactId
{
}

namespace Sample
{
    [_System.Runtime.InteropServices.ComVisible(false)]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1722")]
    [_System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724")]
    [_System.Serializable]
    public partial class SampleLoginData : FootStone.GrainInterfaces.LoginData
    {
        #region Slice data members

        [_System.CodeDom.Compiler.GeneratedCodeAttribute("slice2cs", "3.7.1")]
        public string code;

        #endregion

        partial void ice_initialize();

        #region Constructors

        [_System.CodeDom.Compiler.GeneratedCodeAttribute("slice2cs", "3.7.1")]
        public SampleLoginData() : base()
        {
            this.code = "";
            ice_initialize();
        }

        [_System.CodeDom.Compiler.GeneratedCodeAttribute("slice2cs", "3.7.1")]
        public SampleLoginData(string code)
        {
            this.code = code;
            ice_initialize();
        }

        #endregion

        private const string _id = "::Sample::SampleLoginData";

        public static new string ice_staticId()
        {
            return _id;
        }
        public override string ice_id()
        {
            return _id;
        }

        #region Marshaling support

        [_System.CodeDom.Compiler.GeneratedCodeAttribute("slice2cs", "3.7.1")]
        protected override void iceWriteImpl(Ice.OutputStream ostr_)
        {
            ostr_.startSlice(ice_staticId(), -1, false);
            ostr_.writeString(code);
            ostr_.endSlice();
            base.iceWriteImpl(ostr_);
        }

        [_System.CodeDom.Compiler.GeneratedCodeAttribute("slice2cs", "3.7.1")]
        protected override void iceReadImpl(Ice.InputStream istr_)
        {
            istr_.startSlice();
            code = istr_.readString();
            istr_.endSlice();
            base.iceReadImpl(istr_);
        }

        #endregion
    }
}

namespace Sample
{
}

namespace Sample
{
}

namespace Sample
{
}

namespace Sample
{
}