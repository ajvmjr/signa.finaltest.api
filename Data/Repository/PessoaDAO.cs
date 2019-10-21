using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.TemplateCore.Api.Domain.Entities;

namespace Signa.TemplateCore.Api.Data.Repository
{
    public class PessoaDAO : RepositoryBase
    {
        public PessoaDAO(IConfiguration configuration)
        {
            base.configuration = configuration;
        }

        public int Insert(PessoaEntity pessoa)
        {
            var sql = @"
                    Declare @Id Int

                    Update Infra_Ids
                    Set
                        Pessoa_Id = Pessoa_Id + 1,
                        @Id = Pessoa_Id + 1

                    Insert Into Pessoa (Pessoa_Id, Nome, Nome_Fantasia, Pf_Cpf, Email, Tab_Status_Id)
                    Values (@Id, @Nome, @NomeFantasia, @CnpjCpf, @Email, 1)

                    Select @Id";

            var param = new
            {
                pessoa.Nome,
                pessoa.NomeFantasia,
                CnpjCpf = pessoa.PjCnpj,
                pessoa.Email
            };

            using (var db = Connection)
            {
                return db.QueryFirstOrDefault(sql, param);

                // exemplo procedure
                // return db.QueryFirstOrDefault("Sp_Ecr_Inc_Pessoa_Template", param, commandType: CommandType.StoredProcedure);
            }
        }

        public void Update(PessoaEntity pessoa)
        {
            var sql = @"
                    Update Pessoa
                    Set
                        Nome = @Nome,
                        Nome_Fantasia = @NomeFantasia,
                        PF_CPF = @CnpjCpf,
                        Email = @Email
                    Where Pessoa_Id = @Id";

            var param = new
            {
                pessoa.Id,
                pessoa.Nome,
                pessoa.NomeFantasia,
                cnpjCpf = pessoa.PjCnpj,
                pessoa.Email
            };

            using (var db = Connection)
            {
                db.Execute(sql, param);

                // exemplo procedure
                // return db.QueryFirstOrDefault("Sp_Ecr_Atu_Pessoa_Template", param, commandType: CommandType.StoredProcedure);
            }
        }

        public PessoaEntity GetById(int id)
        {
            var sql = @"
                Select
                    Pessoa_Id as Id,
                    Nome,
                    Nome_Fantasia,
                    Indicativo_Pf_Pj,
                    Pj_Cgc,
                    Pf_Cpf,
                    Email
                From Pessoa
                Where
                    Pessoa_Id = @Id
                And Tab_Status_Id = 1";

            var param = new
            {
                Id = id
            };

            using (var db = Connection)
            {
                return db.QueryFirstOrDefault(sql, param);
            }
        }

        public IEnumerable<PessoaEntity> Get()
        {
            var sql = @"
                Select
                    Pessoa_Id as Id,
                    Nome,
                    Nome_Fantasia,
                    Indicativo_Pf_Pj,
                    Pj_Cgc,
                    Pf_Cpf,
                    Email
                From Pessoa
                Where Tab_Status_Id = 1";

            using (var db = Connection)
            {
                return db.QueryFirstOrDefault(sql);
            }
        }

        public void Delete(int id)
        {
            var sql = "Delete From Pessoa Where Pessoa_Id = @Id";
            var param = new { Id = id };

            using (var db = Connection)
            {
                db.Execute(sql, param);
            }
        }
    }
}