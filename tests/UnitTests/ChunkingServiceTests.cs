using Application.Common;
using FluentAssertions;

namespace UnitTests;

public class ChunkingServiceTests
{
    private readonly ChunkingService _sut = new();

    [Fact]
    public void Chunk_ShortText_ReturnsSingleChunk()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 100));
        var chunks = _sut.Chunk(text, chunkSize: 500, overlap: 50).ToList();
        chunks.Should().HaveCount(1);
    }

    [Fact]
    public void Chunk_LongText_ReturnsMultipleChunks()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 1200));
        var chunks = _sut.Chunk(text, chunkSize: 500, overlap: 50).ToList();
        chunks.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Chunk_OverlapIsApplied_ConsecutiveChunksShareWords()
    {
        var words = Enumerable.Range(1, 600).Select(i => $"w{i}").ToArray();
        var text = string.Join(' ', words);
        var chunks = _sut.Chunk(text, chunkSize: 500, overlap: 50).ToList();

        chunks.Should().HaveCountGreaterThan(1);

        var lastWordsOfFirst = chunks[0].Split(' ').TakeLast(50).ToArray();
        var firstWordsOfSecond = chunks[1].Split(' ').Take(50).ToArray();
        lastWordsOfFirst.Should().BeEquivalentTo(firstWordsOfSecond);
    }

    [Fact]
    public void Chunk_EmptyString_YieldsNoChunks()
    {
        var chunks = _sut.Chunk(string.Empty).ToList();
        chunks.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_ExactlyChunkSizeWords_ReturnsSingleChunk()
    {
        var text = string.Join(' ', Enumerable.Repeat("x", 500));
        var chunks = _sut.Chunk(text, chunkSize: 500, overlap: 50).ToList();
        chunks.Should().HaveCount(1);
        chunks[0].Split(' ').Should().HaveCount(500);
    }

    [Fact]
    public void Chunk_EachChunkDoesNotExceedChunkSize()
    {
        var text = string.Join(' ', Enumerable.Range(1, 2000).Select(i => $"w{i}"));
        var chunks = _sut.Chunk(text, chunkSize: 500, overlap: 50).ToList();

        foreach (var chunk in chunks)
            chunk.Split(' ').Length.Should().BeLessOrEqualTo(500);
    }
}
