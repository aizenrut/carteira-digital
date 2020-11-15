﻿using CarteiraDigital.Dados.Repositorios;
using CarteiraDigital.Models;
using System;

namespace CarteiraDigital.Servicos
{
    public class ContaServico : IContaServico
    {
        private readonly IContaRepositorio contaRepositorio;

        public ContaServico(IContaRepositorio contaRepositorio)
        {
            this.contaRepositorio = contaRepositorio;
        }

        public Conta ObterConta(int contaId)
        {
            ValidarConta(contaId);
            return contaRepositorio.Get(contaId);
        }

        public void ValidarConta(int contaId)
        {
            if(!contaRepositorio.Any(contaId))
                throw new ArgumentException("A conta informada é inválida!");
        }

        public void VincularCashIn(Conta conta, CashIn cashIn)
        {
            conta.CashIns.Add(cashIn);
            contaRepositorio.Update(conta);
        }

        public void VincularCashOut(Conta conta, CashOut cashOut)
        {
            conta.CashOuts.Add(cashOut);
            contaRepositorio.Update(conta);
        }

        public void VincularTransferencia(Conta conta, Transferencia transferencia)
        {
            conta.Transferencias.Add(transferencia);
            contaRepositorio.Update(conta);
        }
    }
}