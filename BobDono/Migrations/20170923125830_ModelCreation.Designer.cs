﻿// <auto-generated />
using BobDono.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace BobDono.Migrations
{
    [DbContext(typeof(BobDatabaseContext))]
    [Migration("20170923125830_ModelCreation")]
    partial class ModelCreation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("BobDono.Entities.Bracket", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("BracketStageId");

                    b.Property<DateTime>("EndDate");

                    b.Property<long?>("FirstWaifuId");

                    b.Property<long?>("SecondWaifuId");

                    b.Property<DateTime>("StartDate");

                    b.HasKey("Id");

                    b.HasIndex("BracketStageId");

                    b.HasIndex("FirstWaifuId");

                    b.HasIndex("SecondWaifuId");

                    b.ToTable("Brackets");
                });

            modelBuilder.Entity("BobDono.Entities.BracketStage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ElectionId");

                    b.HasKey("Id");

                    b.HasIndex("ElectionId");

                    b.ToTable("BracketStage");
                });

            modelBuilder.Entity("BobDono.Entities.Election", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("AuthorId");

                    b.Property<string>("Description");

                    b.Property<ulong>("DiscordChannelId");

                    b.Property<int>("EntrantsPerUser");

                    b.Property<string>("Name");

                    b.Property<DateTime>("SubmissionsEndDate");

                    b.Property<DateTime>("SubmissionsStartDate");

                    b.Property<DateTime>("VotingEndDate");

                    b.Property<DateTime>("VotingStartDate");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Elections");
                });

            modelBuilder.Entity("BobDono.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("DiscordId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BobDono.Entities.UserWaifu", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<long>("WaifuId");

                    b.HasKey("UserId", "WaifuId");

                    b.HasIndex("WaifuId");

                    b.ToTable("UserWaifu");
                });

            modelBuilder.Entity("BobDono.Entities.Vote", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("BracketId");

                    b.Property<long?>("ContenderId");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("BracketId");

                    b.HasIndex("ContenderId");

                    b.HasIndex("UserId");

                    b.ToTable("Votes");
                });

            modelBuilder.Entity("BobDono.Entities.Waifu", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("MalId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Waifus");
                });

            modelBuilder.Entity("BobDono.Entities.WaifuContender", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomImageUrl");

                    b.Property<long?>("ProposerId");

                    b.Property<int>("SeedNumber");

                    b.Property<long?>("WaifuId");

                    b.HasKey("Id");

                    b.HasIndex("ProposerId");

                    b.HasIndex("WaifuId");

                    b.ToTable("WaifuContenders");
                });

            modelBuilder.Entity("BobDono.Entities.Bracket", b =>
                {
                    b.HasOne("BobDono.Entities.BracketStage", "BracketStage")
                        .WithMany("Brackets")
                        .HasForeignKey("BracketStageId");

                    b.HasOne("BobDono.Entities.WaifuContender", "FirstWaifu")
                        .WithMany()
                        .HasForeignKey("FirstWaifuId");

                    b.HasOne("BobDono.Entities.WaifuContender", "SecondWaifu")
                        .WithMany()
                        .HasForeignKey("SecondWaifuId");
                });

            modelBuilder.Entity("BobDono.Entities.BracketStage", b =>
                {
                    b.HasOne("BobDono.Entities.Election", "Election")
                        .WithMany("BracketStages")
                        .HasForeignKey("ElectionId");
                });

            modelBuilder.Entity("BobDono.Entities.Election", b =>
                {
                    b.HasOne("BobDono.Entities.User", "Author")
                        .WithMany("Elections")
                        .HasForeignKey("AuthorId");
                });

            modelBuilder.Entity("BobDono.Entities.UserWaifu", b =>
                {
                    b.HasOne("BobDono.Entities.User", "User")
                        .WithMany("Waifus")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("BobDono.Entities.Waifu", "Waifu")
                        .WithMany("Users")
                        .HasForeignKey("WaifuId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BobDono.Entities.Vote", b =>
                {
                    b.HasOne("BobDono.Entities.Bracket", "Bracket")
                        .WithMany()
                        .HasForeignKey("BracketId");

                    b.HasOne("BobDono.Entities.WaifuContender", "Contender")
                        .WithMany("Votes")
                        .HasForeignKey("ContenderId");

                    b.HasOne("BobDono.Entities.User", "User")
                        .WithMany("Votes")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("BobDono.Entities.WaifuContender", b =>
                {
                    b.HasOne("BobDono.Entities.User", "Proposer")
                        .WithMany()
                        .HasForeignKey("ProposerId");

                    b.HasOne("BobDono.Entities.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuId");
                });
#pragma warning restore 612, 618
        }
    }
}