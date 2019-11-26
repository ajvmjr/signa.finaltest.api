using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Configuration;
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
                    Declare @id Int

                    Update Infra_Ids
                    Set
                        @id = Pessoa_Id + 1,
                        Pessoa_Id = Pessoa_Id + 1

                    Insert Into Pessoa (Pessoa_Id, Nome, Nome_Fantasia, Pf_Cpf, Email, Data_Nascimento, Tab_Status_Id)
                    Values (@id, @nome, @nomeFantasia, @cnpjCpf, @email, @dataNascimento, 1)

                    Select @id";

            var param = new
            {
                nome = new DbString { Value = pessoa.Nome, Length = 250, IsAnsi = true },
                nomeFantasia = new DbString { Value = pessoa.NomeFantasia, Length = 250, IsAnsi = true },
                cnpjCpf = new DbString { Value = pessoa.PjCnpj, Length = 19, IsAnsi = true },
                email = new DbString { Value = pessoa.Email, Length = 100, IsAnsi = true },
                dataNascimento = pessoa.DataNascimento
            };

            using (var db = Connection)
            {
                return db.QueryFirstOrDefault<int>(sql, param);

                // exemplo procedure
                // return db.QueryFirstOrDefault("Sp_Ecr_Inc_Pessoa_Template", param, commandType: CommandType.StoredProcedure);
            }
        }

        public void Update(PessoaEntity pessoa)
        {
            var sql = @"
                    Update Pessoa
                    Set
                        Nome = @nome,
                        Nome_Fantasia = @nomeFantasia,
                        PF_CPF = @cnpjCpf,
                        Email = @email,
                        Data_Nascimento = @dataNascimento
                    Where Pessoa_Id = @id";

            var param = new
            {
                id = pessoa.Id,
                nome = new DbString { Value = pessoa.Nome, Length = 250, IsAnsi = true },
                nomeFantasia = new DbString { Value = pessoa.NomeFantasia, Length = 250, IsAnsi = true },
                cnpjCpf = new DbString { Value = pessoa.PjCnpj, Length = 19, IsAnsi = true },
                email = new DbString { Value = pessoa.Email, Length = 100, IsAnsi = true },
                dataNascimento = pessoa.DataNascimento
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
                    Email,
                    Data_Nascimento
                From Pessoa
                Where
                    Pessoa_Id = @id
                And Tab_Status_Id = 1";

            var param = new
            {
                id
            };

            using (var db = Connection)
            {
                return db.QueryFirstOrDefault<PessoaEntity>(sql, param);
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
                    Email,
                    Data_Nascimento
                From Pessoa
                Where Tab_Status_Id = 1";

            using (var db = Connection)
            {
                return db.Query<PessoaEntity>(sql);
            }
        }

        public void Delete(int id)
        {
            var sql = "Update Pessoa Set Tab_Status_Id = 2 Where Pessoa_Id = @id";
            var param = new { id };

            using (var db = Connection)
            {
                db.Execute(sql, param);
            }
        }
    }
}