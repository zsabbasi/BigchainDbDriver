﻿using BigchainDbDriver.Assets.Models;
using BigchainDbDriver.Assets.Models.TransactionModels;
using BigchainDbDriver.Common;
using BigchainDbDriver.Common.Cryptography;
using BigchainDbDriver.General;
using BigchainDbDriver.KeyPair;
using BigchainDbDriver.Transactions;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace BigchainDbDriver.NUnit.Tests
{
    [TestFixture]
    class Transaction
    {
        private readonly GeneratedKeyPair generatedKeyPair;
        private readonly string bigchainhost = "http://192.168.100.10:9984/api/v1/";

        public Transaction()
        {
            var keypair = new Ed25519Keypair();
            generatedKeyPair = keypair.GenerateKeyPair();
        }


        [Test, Order(1)]
        public void ProvidedInput_Payload_Metadata_Keys_AndMakeCreateTransction()
        {

            Bigchain_Transaction transaction = new Bigchain_Transaction();
            var assets = new Asset
            {
                Assets = new AssetDefinition
                {
                    Data = new DataDefinition
                    {
                        Kyc = new KycDefinition
                        {
                            Dob = "",
                            Nab = "",
                            Pob = "",
                            UserHash = ""
                        }
                    }
                }
            };

            var metadata = new Metadata
            {
                Error = "",
                Status = "",
                Transaction = ""
            };

            TxTemplate txTemplate = transaction.MakeCreateTransaction(assets,
                metadata,
                transaction.MakeOutput(transaction.MakeEd25519Condition(generatedKeyPair.PublicKey)),
                new List<string> { generatedKeyPair.PublicKey }
                );

            Assert.AreEqual(generatedKeyPair.PublicKey, txTemplate.Outputs[0].PublicKeys[0]);
            Assert.AreEqual(generatedKeyPair.PublicKey, txTemplate.Inputs[0].Owners_before[0]);
        }

        [Test]
        public void ProvidedPubKey_ShouldGeneratedValidCcUrl() {
            var pubKey = "WuD9VBm3kAUKkZ2Cvvij4QsfkGFqxvfX6qGg6qQxsZs";
            var expectedUri = "dCQ-qJBCsSNC6AGifLWu0Cuhv38V707Tk0C8TdR-R1k";

            var generatedUri = Base64Url.Encode(pubKey.ToByteArray());

            Assert.AreEqual(expectedUri, generatedUri);
        }

        [Test, Order(2)]
        public async Task ProvidedSignedTx_ShouldPostCommitTransaction() {
            var signedTx = GetMockResponseSignedTx();
            
            var connection = new BigchainConnection(bigchainhost);
            var (response,status) = await connection.PostTransactionCommit(signedTx);

            Assert.AreNotEqual(status, HttpStatusCode.BadRequest);
            Assert.That(status == HttpStatusCode.Accepted || status == HttpStatusCode.Created || status == HttpStatusCode.NoContent);
        }

        [Test]
        public void ProvidedTx_ShouldReturnValidSignedTx() {
            var keypairgenerator = new Ed25519Keypair();
            var keypair = keypairgenerator.GenerateKeyPair();
            var expectedHash = "0b876b6a1604f6f313e63640d6f90eb09d85d56c2036034bc7dbf039cf585f33";

            var tx = GetMockResponseTx(keypair.PublicKey);
            var signTx = new Bigchain_SignTransaction();
            var signedTx = signTx.SignTransaction(tx, new List<string> { $"{keypair.ExpandedPrivateKey}" });
            var serializedTx = JsonUtility.SerializeTransactionIntoCanonicalString(JsonConvert.SerializeObject(signedTx));

            Assert.AreEqual(expectedHash, signedTx.Id);

        }

        [Test]
        public void Provided_Fulfillment_Should_Return_DerEncodedFullfillment() {
            var expectedFulfillment = "pGSAIOwsDX_8KpzAef-aHlT1QXPnf23YDNEHK26-hw9xtTgEgUDhORNF-ZyNX9_Ymdukyxit-tWFur2OFZokgxD97_Mzt7C67cDhL9P-FelNFJV0srFaGxmw5fQ1kRYTemee3P4J";
            var transactionHash = "28a985bcf3b46a6895035b9f0fb7962190f76316eb46c5a0f3450195200b5780";
            var fulfillment = "ni:///sha-256;NAgseHeCPxu1v5vqPE-mF_IFk6EqBdk7YuAW3LltFAM?fpt=ed25519-sha-256&cost=131072";
            var signedTxId = "f84adc4d2dc630f4f3380b94bd82a196e40907bb55cddb7822842703c789246d";
            var keyPair = new GeneratedKeyPair()
            {
                PrivateKey = "8hiZ8FPQLQnmFqXg8T1L3tgkJvLPeZXnGuThprDDJtQR",
                PublicKey = "GtvBGsnVhGnqR1RswqT3KSwdoU3UW7w23ukmDaH7uAEF"
            };

            var signTx = new Bigchain_SignTransaction();
            var fulfillmentUri = signTx.GenerateFulfillmentUri(keyPair.PublicKey);

            Assert.AreEqual(expectedFulfillment, fulfillmentUri);

        }

        [Test]
        public void ProvidedString_ShouldReturnValidSha256Hash() {
            var stringToHash = "abc";
            var expectedHash = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

            var sha = new Sha256();

            var actualHash = sha.SHA256HexHashString(stringToHash);

            Assert.AreEqual(expectedHash, actualHash);
        }

        [Test]
        public void Provided_PublicKey_Should_Return_Valid_Bytes() {
            byte[] expectedBytes = new byte[] {
                236,44,13,127,252,42,156,192,121,255,154,30,84,245,65,115,231,127,109,216,12,209,7,43,110,190,135,15,113,181,56,4
            };
            var pubKey = "GtvBGsnVhGnqR1RswqT3KSwdoU3UW7w23ukmDaH7uAEF";
            DataEncoder Encoder = Encoders.Base58;

            Assert.AreEqual(expectedBytes, Encoder.DecodeData(pubKey));

        }

        [Test]
        public void ProvidedString_ShouldReturnValidSha3256() {
            var stringToHash = "{\"asset\":{\"data\":{\"kyc\":{\"dob\":\"7/19/1988 12:00:00 AM +05:00\",\"nab\":\"Hang MioLoi\",\"pob\":\"CN\",\"user_hash\":\"5c9b0ddd16f0d6471c661c0e\"}}},\"id\":null,\"inputs\":[{\"fulfillment\":null,\"fulfills\":null,\"owners_before\":[\"GtvBGsnVhGnqR1RswqT3KSwdoU3UW7w23ukmDaH7uAEF\"]}],\"metadata\":{\"Error\":null,\"Status\":\"A\",\"Transaction\":null},\"operation\":\"CREATE\",\"outputs\":[{\"amount\":\"1\",\"condition\":{\"details\":{\"public_key\":\"GtvBGsnVhGnqR1RswqT3KSwdoU3UW7w23ukmDaH7uAEF\",\"type\":\"ed25519-sha-256\"},\"uri\":\"ni:///sha-256;GtvBGsnVhGnqR1RswqT3KSwdoU3UW7w23ukmDaH7uAEF?fpt=ed25519-sha-256&cost=131072\"},\"public_keys\":[\"GtvBGsnVhGnqR1RswqT3KSwdoU3UW7w23ukmDaH7uAEF\"]}],\"version\":\"2.0\"}";
            var expectedHash = "38f4e09c71930ad235bf89f9772845510832a20ae54cfa1a3ab766531b87837a";

            var sha3 = new Sha3_256();

            var actualHash = sha3.ComputeHash(stringToHash);
            Assert.AreEqual(expectedHash, actualHash);
        }


        private TxTemplate GetMockResponseTx(string pubKey)
        {
            return new TxTemplate
            {
                Id = null,
                Asset = new AssetDefinition
                {
                    Data = new DataDefinition
                    {
                        Kyc = new KycDefinition
                        {
                            Dob = "7/19/1988 12:00:00 AM +05:00",
                            Nab = "Hang MioLoi",
                            Pob = "CN",
                            UserHash = "5c9b0ddd16f0d6471c661c0e"
                        }
                    }

                },
                Inputs = new List<InputTemplate>() {
                    new InputTemplate {
                        Fulfills = null,
                        Fulfillment = null,
                        Owners_before = new List<string>() { pubKey }
                    }
                },
                Metadata = new Metadata
                {
                    Error = null,
                    Status = "A",
                    Transaction = null
                },
                Operation = "CREATE",
                Version = "2.0",
                Outputs = new List<Output>() {
                    new Output{
                        Amount = "1",
                        Condition = new MakeEd25519Condition{
                            Details = new Details{
                                PublicKey = pubKey,
                                Type = "ed25519-sha-256"
                            },
                             Uri = pubKey.GenerateMockUri()

                        },
                        PublicKeys = new List<string>(){
                            pubKey
                        }
                    }
                }
            };
        }

        private SignedTxResponse GetMockResponseSignedTx() {
            return new SignedTxResponse
            {
                id = "d48b333ea27d60dae01546a3a184d532e7fad7c7545335ac7d0a32b0fe517a71",
                asset =  new AssetDefinition {
                        Data = new DataDefinition {
                            Kyc = new KycDefinition {
                                Dob = "11/23/1995 12:00:00 AM +00:00",
                                Nab = "JohnDoe2",
                                Pob = "PK",
                                UserHash = "5c86551688dbd41fdc9ed303"
                            }
                        }
                    
                },
                inputs = new List<InputTemplate>() {
                   new InputTemplate{
                       Fulfills = null,
                       Fulfillment = "pGSAIMaPmqqCAswrUdxfzjgqRQGaIQN8M3yBO2LJoSlZRQXxgUBUm9G4vE7Xy-b4YbHyYAYQOSUJBi5ejXRExz9rflb4LVx6wYgrewwR89TeLC-HeuxbjuckZj7-z37NDPXaw8EB",
                       Owners_before = new List<string>(){ "EN6jFN4LAaBnzkZQekdzYU5XUTyKKX5EiUUBnFgfkozQ" }
                   }
                },
                metadata = new Metadata
                {
                    Error = null,
                    Status = "A",
                    Transaction = null
                },
                operation = "CREATE",
                version = "2.0",
                outputs = new List<Output>() {
                    new Output{
                        Amount = "1",
                        Condition = new MakeEd25519Condition{
                            Details = new Details{
                                PublicKey = "EN6jFN4LAaBnzkZQekdzYU5XUTyKKX5EiUUBnFgfkozQ",
                                Type = "ed25519-sha-256"
                            },
                             Uri = "ni:///sha-256;sAdXqonGQXqcDfhFR8JchTEYlBXvn15Z_QnEOV-8j5I?fpt=ed25519-sha-256&cost=131072"

                        },
                        PublicKeys = new List<string>(){
                            "EN6jFN4LAaBnzkZQekdzYU5XUTyKKX5EiUUBnFgfkozQ"
                        }
                    }
                }
            };
        }

    }
}
