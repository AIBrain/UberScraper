﻿#region License & Information

// This notice must be kept visible in the source.
//
// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified,
// or the original license has been overwritten by the automatic formatting of this code.
// Any unmodified sections of source code borrowed from other projects retain their original license and thanks goes to the Authors.
//
// Donations and Royalties can be paid via
// PayPal: paypal@aibrain.org
// bitcoin:1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
// bitcoin:1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
// litecoin:LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
//
// Usage of the source code or compiled binaries is AS-IS.
// I am not responsible for Anything You Do.
//
// Contact me by email if you have any questions or helpful criticism.
//
// "UberScraper/BitcoinFaucets.cs" was last cleaned by Rick on 2014/09/13 at 4:05 PM

#endregion License & Information

namespace UberScraper {

    using System.ComponentModel;

    public enum BitcoinFaucets {

        [Description( "about:blank" )]
        AboutBlank,

        [Description( "http://AIBrain.Org/bitcoin/?a=1MpfkH1vDyGrmtykodJmzBNWi81KqXa8SE" )]
        AIBrainDotOrg,

        //[Description( "http://www.bitchest.me/" )]
        //BitChestDotMe,    //dead faucet

        [Description( "https://www.landofbitcoin.com/" )]
        LandOfBitCoinDotCom,


    }
}