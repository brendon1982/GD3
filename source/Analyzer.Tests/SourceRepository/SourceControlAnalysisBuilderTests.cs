﻿using Analyzer.Data.SourceControl;
using Analyzer.Data.Test.Utils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;

namespace Analyzer.Data.Tests.SourceRepository
{
    [TestFixture]
    public class SourceControlAnalysisBuilderTests
    {
        [Test]
        public void WhenPathNotValidGitRepo_ShouldThrowException()
        {
            // arrange
            var repoPath = "x:\\invalid_repo";
            var builder = new SourceControlAnalysisBuilder()
                .WithPath(repoPath);
            // act
            var actual = Assert.Throws<Exception>(() => builder.Build());
            // assert
            var expected = "Invalid path [x:\\invalid_repo]";
            actual.Message.Should().Be(expected);
        }

        [Test]
        public void WhenInvalidBranch_ShouldReturnDeveloperList()
        {
            // arrange
            var context = new RepositoryTestDataBuilder().Build();
            using (context)
            {
                var sut = new SourceControlAnalysisBuilder()
                    .WithPath(context.Path)
                    .WithRange(DateTime.Parse("2018-06-25"), DateTime.Parse("2018-07-09"))
                    .WithBranch("--Never-Existed--");
                // act
                var actual = Assert.Throws<Exception>(() => sut.Build());
                // assert
                actual.Message.Should().Be("Invalid branch [--Never-Existed--]");
            }
        }

        [Test]
        public void WhenNoRangeSpecified_ShouldUseRepositorysFirstAndLastCommitDates()
        {
            // arrange
            var repoPath = TestRepoPath("git-test-operations");
            //var context = new RepositoryTestDataBuilder()
            //              .With_Commit(new TestCommit { FileName = "file1.txt", Lines = new List<string> { "1", "2" }, TimeStamp = "2018-07-16" })
            //              .With_Commit(new TestCommit { FileName = "file2.txt", Lines = new List<string> { "3", "4" }, TimeStamp = "2018-09-13" })
            //              .Build();
            var sut = new SourceControlAnalysisBuilder()
                .WithPath(repoPath)
                .WithEntireHistory()
                .Build();
            // act
            var actual = sut.ReportingRange;
            // assert
            actual.Start.Should().Be(DateTime.Parse("2018-07-16"));
            actual.End.Should().Be(DateTime.Parse("2018-09-25"));
        }

        [Test]
        public void WhenNullIgnorePatterns_ShouldNotThrowException()
        {
            // arrange
            var repoPath = TestRepoPath("git-test-operations");
            var sut = new SourceControlAnalysisBuilder()
                .WithPath(repoPath)
                .WithIgnorePatterns(null);
            // act
            // assert
            Assert.DoesNotThrow(() => sut.Build());
        }

        private static string TestRepoPath(string repo)
        {
            var basePath = TestContext.CurrentContext.TestDirectory;
            var rootPath = GetRootPath(basePath);
            var repoPath = Path.Combine(rootPath, repo);
            return repoPath;
        }

        private static string GetRootPath(string basePath)
        {
            var source = "source";
            var indexOf = basePath.IndexOf(source, StringComparison.Ordinal);
            var rootPath = basePath.Substring(0, indexOf);
            return rootPath;
        }
    }
}
