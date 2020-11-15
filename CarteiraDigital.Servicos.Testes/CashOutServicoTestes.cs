﻿using CarteiraDigital.Dados.Repositorios;
using CarteiraDigital.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;

namespace CarteiraDigital.Servicos.Testes
{
    [TestClass]
    public class CashOutServicoTestes
    {
        [TestMethod]
        public void GerarCashOut_CashOutValido_DeveRetornarOCashOutECalcularTaxa()
        {
            // Arrange
            var conta = new Conta { Id = 1 };
            var valor = 10m;
            var descricao = "Teste unitário.";

            var cashOutServico = new CashOutServico(null, null, null);

            // Act
            var result = cashOutServico.GerarCashOut(conta, valor, descricao);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(valor, result.Valor);
            Assert.AreEqual(0.1m, result.ValorTaxa);
            Assert.AreEqual(descricao, result.Descricao);
            Assert.AreNotEqual(default, result.DataEfetivacao);
        }

        [TestMethod]
        public void Efetivar_SaldoSuficiente_DeveRetornarOCashOut()
        {
            // Arrange
            var conta = new Conta
            {
                Id = 1,
                Saldo = 100
            };

            var valor = 10m;
            var descricao = "Teste unitário.";
            CashOut cashOutGerado = null;

            var cashOutRepositorio = Substitute.For<ICashOutRepositorio>();
            cashOutRepositorio.When(x => x.Post(Arg.Any<CashOut>()))
                              .Do(x => cashOutGerado = x.Arg<CashOut>());

            var contaRepositorio = Substitute.For<IContaRepositorio>();
            contaRepositorio.Any(conta.Id).Returns(true);
            contaRepositorio.Get(conta.Id).Returns(conta);

            var contaServico = new ContaServico(contaRepositorio);
            var operacaoServico = new OperacaoServico();

            var dto = new OperacaoUnariaDto(conta.Id, valor, descricao);

            var cashOutServico = new CashOutServico(cashOutRepositorio, operacaoServico, contaServico);

            // Act
            cashOutServico.Efetivar(dto);

            // Assert
            Assert.IsNotNull(cashOutGerado);
            Assert.AreEqual(valor, cashOutGerado.Valor);
            Assert.AreEqual(0.1m, cashOutGerado.ValorTaxa);
            Assert.AreEqual(descricao, cashOutGerado.Descricao);
            Assert.AreNotEqual(default, cashOutGerado.DataEfetivacao);
            cashOutRepositorio.Received(1).Post(Arg.Any<CashOut>());
            Assert.IsTrue(conta.CashOuts.Contains(cashOutGerado));
        }

        [TestMethod]
        public void Efetivar_ContaInvalida_DeveLancarExcecaoEPararOProcesso()
        {
            // Arrange
            var conta = new Conta { Id = 1 };
            var valor = 10m;
            var descricao = "Teste unitário.";
            CashOut cashOutGerado = null;

            var cashOutRepositorio = Substitute.For<ICashOutRepositorio>();
            cashOutRepositorio.When(x => x.Post(Arg.Any<CashOut>()))
                              .Do(x => cashOutGerado = x.Arg<CashOut>());

            var contaRepositorio = Substitute.For<IContaRepositorio>();
            contaRepositorio.Any(conta.Id).Returns(false);

            var contaServico = new ContaServico(contaRepositorio);
            var operacaoServico = new OperacaoServico();

            var dto = new OperacaoUnariaDto(conta.Id, valor, descricao);

            var cashOutServico = new CashOutServico(cashOutRepositorio, operacaoServico, contaServico);

            // Act
            Action acao = () => cashOutServico.Efetivar(dto);

            // Assert
            var excecao = Assert.ThrowsException<ArgumentException>(acao);
            Assert.IsTrue(excecao.Message.Contains("A conta informada é inválida!"));
            Assert.IsNull(cashOutGerado);
            cashOutRepositorio.Received(0).Post(Arg.Any<CashOut>());
            Assert.AreEqual(0, conta.CashOuts.Count);
        }

        [TestMethod]
        public void Efetivar_ValorInvalido_DeveLancarExcecaoEPararOProcesso()
        {
            // Arrange
            var conta = new Conta { Id = 1 };
            var valor = 0;
            var descricao = "Teste unitário.";
            CashOut cashOutGerado = null;

            var cashOutRepositorio = Substitute.For<ICashOutRepositorio>();
            cashOutRepositorio.When(x => x.Post(Arg.Any<CashOut>()))
                              .Do(x => cashOutGerado = x.Arg<CashOut>());

            var contaRepositorio = Substitute.For<IContaRepositorio>();
            contaRepositorio.Any(conta.Id).Returns(true);
            contaRepositorio.Get(conta.Id).Returns(conta);

            var contaServico = new ContaServico(contaRepositorio);
            var operacaoServico = new OperacaoServico();

            var dto = new OperacaoUnariaDto(conta.Id, valor, descricao);

            var cashOutServico = new CashOutServico(cashOutRepositorio, operacaoServico, contaServico);

            // Act
            Action acao = () => cashOutServico.Efetivar(dto);

            // Assert
            var excecao = Assert.ThrowsException<ArgumentException>(acao);
            Assert.IsTrue(excecao.Message.Contains("O valor da operação deve ser superior a zero!"));
            Assert.IsNull(cashOutGerado);
            cashOutRepositorio.Received(0).Post(Arg.Any<CashOut>());
            Assert.AreEqual(0, conta.CashOuts.Count);
        }
    }
}
