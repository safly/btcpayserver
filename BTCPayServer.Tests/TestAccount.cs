﻿using BTCPayServer.Controllers;
using BTCPayServer.Invoicing;
using BTCPayServer.Models.AccountViewModels;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBitpayClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BTCPayServer.Tests
{
    public class TestAccount
    {
		ServerTester parent;
		public TestAccount(ServerTester parent)
		{
			this.parent = parent;
			BitPay = new Bitpay(new Key(), parent.PayTester.ServerUri);
		}

		public void GrantAccess()
		{
			GrantAccessAsync().GetAwaiter().GetResult();
		}
		public async Task GrantAccessAsync()
		{
			var extKey = new ExtKey().GetWif(parent.Network);
			var pairingCode = BitPay.RequestClientAuthorization("test", Facade.Merchant);
			var account = parent.PayTester.GetController<AccountController>();
			await account.Register(new RegisterViewModel()
			{
				Email = "Bob@toto.com",
				ConfirmPassword = "Kitten0@",
				Password = "Kitten0@",
			});
			UserId = account.RegisteredUserId;
			StoreId = account.RegisteredStoreId;
			var manage = parent.PayTester.GetController<ManageController>(account.RegisteredUserId);
			await manage.Index(new Models.ManageViewModels.IndexViewModel()
			{
				ExtPubKey = extKey.Neuter().ToString(),
				SpeedPolicy = SpeedPolicy.MediumSpeed
			});
			Assert.IsType<ViewResult>(await manage.AskPairing(pairingCode.ToString()));
			await manage.Pairs(pairingCode.ToString());
		}

		public Bitpay BitPay
		{
			get; set;
		}
		public string UserId
		{
			get; set;
		}

		public string StoreId
		{
			get; set;
		}
	}
}
