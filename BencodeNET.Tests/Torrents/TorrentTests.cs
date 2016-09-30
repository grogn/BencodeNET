﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Torrents;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Torrents
{
    public class TorrentTests
    {
        [Fact]
        public void FileMode_IsSingleFile_IfFilesIsEmptyAndFileIsNotNull()
        {
            var torrent = new Torrent
            {
                File = new SingleFileInfo(),
                Files = new MultiFileInfoList()
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Single);
        }

        [Fact]
        public void FileMode_IsMultiFile_IfFilesIsNotEmpty()
        {
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList
                {
                    new MultiFileInfo()
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Multi);
        }

        [Fact]
        public void FileMode_IsUnknown_IfFileIsNullAndFilesIsEmpty()
        {
            var torrent = new Torrent
            {
                File = null,
                Files = new MultiFileInfoList()
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Unknown);
        }

        [Theory]
        [AutoMockedData]
        public void DisplayName_SingleFile_IsFileName(string fileName)
        {
            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileName = fileName
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Single);
            torrent.DisplayName.Should().Be(fileName);
        }

        [Theory]
        [AutoMockedData]
        public void DisplayName_MultiFile_IsDirectoryName(string directoryName)
        {
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList(directoryName)
                {
                    new MultiFileInfo()
                }
            };

            torrent.FileMode.Should().Be(TorrentFileMode.Multi);
            torrent.DisplayName.Should().Be(directoryName);
        }

        [Fact]
        public void DisplayName_UnknownFileMode_ThrowsBencodeException()
        {
            var torrent = new Torrent();

            Func<string> act = () => torrent.DisplayName;

            torrent.FileMode.Should().Be(TorrentFileMode.Unknown);
            act.Invoking(x => x()).ShouldThrow<BencodeException>();
        }

        [Theory]
        [AutoMockedData]
        public void TotalSize_SingleFile_ShouldBeFileSize(long fileSize)
        {
            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileSize = fileSize
                }
            };

            torrent.TotalSize.Should().Be(fileSize);
        }

        [Theory]
        [AutoMockedData]
        public void TotalSize_MultiFile_ShouldBeSumOfFileSizes(long fileSize1, long fileSize2)
        {
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList
                {
                    new MultiFileInfo {FileSize = fileSize1},
                    new MultiFileInfo {FileSize = fileSize2},
                }
            };

            torrent.TotalSize.Should().Be(fileSize1 + fileSize2);
        }

        [Fact]
        public void TotalSize_UnknownFileMode_IsZero()
        {
            var torrent = new Torrent();

            torrent.TotalSize.Should().Be(0);
        }

        [Theory]
        [AutoMockedData]
        public void NumberOfPieces_ShouldBeTotalSizeDividedByPieceSizeRoundedUp(long fileSize, long pieceSize)
        {
            var expected = (int)Math.Ceiling((double) fileSize/pieceSize);

            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileSize = fileSize
                },
                PieceSize = pieceSize
            };

            torrent.TotalSize.Should().Be(fileSize);
            torrent.NumberOfPieces.Should().Be(expected);
        }

        [Fact]
        public void ToBDictionary_Encoding_UsesWebNameToUpper()
        {
            var torrent = new Torrent { Encoding = Encoding.UTF8 };

            var result = torrent.ToBDictionary();

            result.Get<BString>(TorrentFields.Encoding).Should().Be(Encoding.UTF8.WebName.ToUpper());
        }

        [Fact]
        public void ToBDictionary_Encoding_AddsToCorrectField()
        {
            var torrent = new Torrent {Encoding = Encoding.UTF8};

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.Encoding, (BString)Encoding.UTF8.WebName.ToUpper());
        }

        [Fact]
        public void ToBDictionary_Encoding_DoesNotAddNull()
        {
            var torrent = new Torrent {Encoding = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.Encoding);
        }

        [Fact]
        public void ToBDictionary_Announce_NoTrackers_DoesNotAddAnnounce()
        {
            var torrent = new Torrent
            {
                Trackers = new List<IList<string>>
                {
                    new List<string>()
                }
            };

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.Announce);
        }

        [Fact]
        public void ToBDictionary_Announce_SingleTracker_AddsAnnounceButNotAddAnnounceList()
        {
            var torrent = new Torrent
            {
                Trackers = new List<IList<string>>
                {
                    new List<string>
                    {
                        "http://sometracker.com"
                    }
                }
            };

            var result = torrent.ToBDictionary();

            result.Should().ContainKey(TorrentFields.Announce);
            result.Should().NotContainKey(TorrentFields.AnnounceList);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_Comment_AddsToCorrectField(string comment)
        {
            var torrent = new Torrent {Comment = comment
            };

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.Comment, (BString)comment);
        }

        [Fact]
        public void ToBDictionary_Comment_DoesNotAddNull()
        {
            var torrent = new Torrent {Comment = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.Comment);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_CreatedBy_AddsToCorrectField(string createdBy)
        {
            var torrent = new Torrent {CreatedBy = createdBy};

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.CreatedBy, (BString)createdBy);
        }

        [Fact]
        public void ToBDictionary_CreatedBy_DoesNotAddNull()
        {
            var torrent = new Torrent {CreatedBy = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.CreatedBy);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_CreationDate_AddsToCorrectField(DateTime creationDate)
        {
            var torrent = new Torrent {CreationDate = creationDate};

            var result = torrent.ToBDictionary();

            result.Should().Contain(TorrentFields.CreationDate, (BNumber)creationDate);
        }

        [Fact]
        public void ToBDictionary_CreationDate_DoesNotAddNull()
        {
            var torrent = new Torrent {CreationDate = null};

            var result = torrent.ToBDictionary();

            result.Should().NotContainKey(TorrentFields.CreationDate);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_Info_SingleFile_AddsNameAndLength(long fileSize, string fileName)
        {
            // Arrange
            var torrent = new Torrent
            {
                File = new SingleFileInfo
                {
                    FileSize = fileSize,
                    FileName = fileName
                }
            };

            // Act
            var info = torrent.ToBDictionary().Get<BDictionary>("info");

            // Assert
            info.Should().Contain(TorrentFields.Length, (BNumber)fileSize);
            info.Should().Contain(TorrentFields.Name, (BString)fileName);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_Info_MultiFile_AddsFiles(string directoryName, long fileSize, IList<string> path)
        {
            // Arrange
            var torrent = new Torrent
            {
                Files = new MultiFileInfoList(directoryName)
                {
                    new MultiFileInfo
                    {
                        FileSize = fileSize,
                        Path = path
                    }
                }
            };

            // Act
            var info = torrent.ToBDictionary().Get<BDictionary>("info");
            var files = info.Get<BList<BDictionary>>(TorrentFields.Files);

            // Assert
            info.Should().Contain(TorrentFields.Name, (BString) directoryName);
            info.Should().ContainKey(TorrentFields.Files);
            info[TorrentFields.Files].Should().BeOfType<BList<BDictionary>>();
            files[0].Should().BeOfType<BDictionary>();
            files[0].Should().Contain(TorrentFields.Length, (BNumber) fileSize);
            files[0].Should().ContainKey(TorrentFields.Path);
            files[0].Get<BList>(TorrentFields.Path).Should().HaveCount(path.Count);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_ExtraFields_AreAdded(BString key, BString value)
        {
            // Arrange
            var torrent = new Torrent
            {
                ExtraFields = new BDictionary
                {
                    [key] = value
                }
            };

            // Act
            var result = torrent.ToBDictionary();

            // Assert
            result.Should().Contain(key, value);
        }

        [Theory]
        [AutoMockedData]
        public void ToBDictionary_ExtraFields_OverwritesExistingData(string comment, BString extraValue)
        {
            // Arrange
            var torrent = new Torrent
            {
                Comment = comment,
                ExtraFields = new BDictionary
                {
                    {TorrentFields.Comment, extraValue}
                }
            };

            // Act
            var result = torrent.ToBDictionary();

            // Assert
            result.Should().Contain(TorrentFields.Comment, extraValue);
        }

        [Fact]
        public void ToBDictionary_EqualsEmptyBDictionaryForEmptyTorrent()
        {
            var torrent = new Torrent();
            var result = torrent.ToBDictionary();
            result.Should().HaveCount(0);
            result.ShouldBeEquivalentTo(new BDictionary());
        }

        [Theory]
        [AutoMockedData]
        public void Equals_ComparesContent(long fileSize, string fileName, string comment, string createdBy, DateTime creationDate, long pieceSize, IList<IList<string>> trackers)
        {
            var t1 = new Torrent
            {
                Encoding = Encoding.UTF8,
                File = new SingleFileInfo
                {
                    FileSize = fileSize,
                    FileName = fileName
                },
                Comment = comment,
                CreatedBy = createdBy,
                CreationDate = creationDate,
                IsPrivate = true,
                PieceSize = pieceSize,
                Trackers = trackers
            };

            var t2 = new Torrent
            {
                Encoding = Encoding.UTF8,
                File = new SingleFileInfo
                {
                    FileSize = fileSize,
                    FileName = fileName
                },
                Comment = comment,
                CreatedBy = createdBy,
                CreationDate = creationDate,
                IsPrivate = true,
                PieceSize = pieceSize,
                Trackers = trackers
            };

            t1.Equals(t2).Should().BeTrue();
            t2.Equals(t1).Should().BeTrue();
            (t1 == t2).Should().BeTrue();
        }
    }
}