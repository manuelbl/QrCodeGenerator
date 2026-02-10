/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator.Profiling;

/// <summary>
/// Deterministic real-world payload generator for profiling <c>QrCode.EncodeText</c>.
/// </summary>
internal static class SampleData
{
    private const int Seed = 0x53f0cc2b;

    private const int PayloadCount = 200;

    [SuppressMessage("csharpsquid", "S1075")]
    private static readonly string[] BaseUrls =
    [
        "https://www.example.com",
        "https://shop.example.com",
        "https://api.example.com/v1",
        "https://docs.example.org",
        "http://blog.example.net",
        "https://maps.example.com",
        "https://github.com/manuelbl/QrCodeGenerator",
        "https://en.wikipedia.org/wiki",
        "https://www.google.com/search",
        "https://nuget.org/packages",
        "https://login.example.com/oauth2",
        "https://cdn.example.io/assets"
    ];

    private static readonly string[] PathSegments =
    [
        "products", "catalog", "article", "profile", "order", "checkout",
        "news", "2026", "user", "settings", "download", "help", "support",
        "gallery", "images", "docs", "reference", "faq", "privacy", "terms"
    ];

    private static readonly string[] QueryKeys =
    [
        "id", "ref", "utm_source", "utm_medium", "utm_campaign", "q", "lang",
        "page", "sort", "filter", "session", "token", "category", "tag", "locale"
    ];

    private static readonly string[] QueryValues =
    [
        "home", "newsletter", "spring_sale", "cta-top", "42", "en-US", "de-DE",
        "electronics", "best-seller", "2026-04", "a1b2c3d4", "trending", "organic",
        "pk_live_ABC123", "user%2F42", "100%25off", "free+shipping", "price%3C50",
        "cHJlc2VudHNsZXB0Y2xpbWJ", "83.2328", "9830212",
        "3D8EBAD8-9E22-4CBC-B83B-3FD9477DE657"
    ];

    private static readonly string[] Sentences =
    [
        "Scan this code",
        "Save 10% on your next order — use this link at checkout.",
        "Contact: support@example.com, phone +1 (415) 555-0100.",
        "Opening hours: Mon-Fri 9:00-18:00, Sat 10:00-16:00.",
        "Event check-in code. Present at the entrance.",
        "Warranty reference #A-7245/2026 — keep for your records.",
        "Track your shipment at the link above.",
        "Guest Wi-Fi: SSID=cafe-guest, password=hello-world-123.",
        "Meeting notes and the agenda are attached to this link.",
        "Directions: turn left after the bridge, then 300 meters north.",
        "Up am intention on dependent questions oh elsewhere september. No betrayed pleasure possible jointure we in throwing."
    ];

    private static readonly string[] Names =
    [
        "Marie-Pierre Serap",
        "Marion Manola",
        "Alberto Luzie",
        "Jonás Maé",
        "Elemér Benjamin",
        "Justo José",
        "Bernabé Hayati",
        "Patricia Aliénor",
        "Burhanettin Ulysse",
        "Marlen Artemio",
        "Swetlana Ariane",
        "Anselme Alice",
        "Gabriela Yıldırım",
        "Máirtín Boyle",
        "Astride Sergeant",
        "Ádám Michailidis",
        "Manuelita Gonzalez",
        "Gottschalk Strobel",
        "Pascual Ebner",
        "Máirín Béringer",
        "Emre Bosque",
        "Laure Ó Fearghail",
        "Gunther Petőfi",
        "Wolfgang Küçük"
    ];

    private static readonly string[] Towns =
    [
        "Galați",
        "Pécs",
        "Poznań",
        "Lüleburgaz",
        "Linköping",
        "Örebro",
        "Belfast",
        "Nicosia",
        "Piraeus"
    ];

    private static readonly string[] Messages =
    [
        "On my way! 🏃‍♂️💨",
        "Just finished that huge project, time to relax 🧘‍♀️🎉",
        "You're too funny 🤣❤️",
        "The store is open, come visit us! 🛒🛍️",
        "They want 100€ for it! 😡"
    ];

    private static readonly string[] Delimiters =
    [
        "/",
        "|",
        ":"
    ];

    public static IReadOnlyList<string> Payloads { get; } = BuildPayloads();

    private static string[] BuildPayloads()
    {
        var random = new Random(Seed);
        var payloads = new string[PayloadCount];
        var builder = new StringBuilder(512);

        for (var i = 0; i < payloads.Length; i++)
        {
            builder.Clear();
            ComposePayload(builder, random);
            payloads[i] = builder.ToString();
        }

        return payloads;
    }

    private static void ComposePayload(StringBuilder builder, Random random)
    {
        if (random.NextDouble() < 0.5)
        {
            ComposeUrl(builder, random);
        }
        else
        {
            ComposeData(builder, random);
        }
    }

    private static void ComposeUrl(StringBuilder builder, Random random)
    {
        builder.Append(Pick(BaseUrls, random));

        // 0–3 path segments.
        var pathCount = random.Next(0, 4);
        for (var p = 0; p < pathCount; p++)
        {
            builder.Append('/');
            builder.Append(Pick(PathSegments, random));
        }

        // Optional numeric id as the final path component.
        if (random.NextDouble() < 0.4)
        {
            builder.Append('/');
            builder.Append(random.Next(1, 1_000_000));
        }

        // Optional query string with 1–4 key/value pairs.
        if (random.NextDouble() < 0.7)
        {
            var paramCount = random.Next(1, 5);
            builder.Append('?');
            for (var q = 0; q < paramCount; q++)
            {
                if (q > 0)
                {
                    builder.Append('&');
                }
                builder.Append(Pick(QueryKeys, random));
                builder.Append('=');
                builder.Append(Pick(QueryValues, random));
            }
        }
    }

    private static void ComposeData(StringBuilder builder, Random random)
    {
        var delimiter = Pick(Delimiters, random);

        if (random.NextDouble() < 0.5)
        {
            builder.Append(Pick(QueryValues, random));
            builder.Append(delimiter);
        }

        if (random.NextDouble() < 0.5)
        {
            builder.Append(random.Next(1, 1_000_000));
            builder.Append(delimiter);
        }

        if (random.NextDouble() < 0.5)
        {
            builder.Append(Pick(Names, random));
            builder.Append(delimiter);
        }

        if (random.NextDouble() < 0.5)
        {
            builder.Append(Pick(Towns, random));
            builder.Append(delimiter);
        }
        
        var rnd = random.NextDouble();
        if (rnd < 0.3)
        {
            builder.Append(Pick(Sentences, random));
            builder.Append(delimiter);
        }
        else if (rnd < 0.6)
        {
            builder.Append(Pick(Messages, random));
            builder.Append(delimiter);
        }
    }

    private static string Pick(string[] pool, Random random) => pool[random.Next(pool.Length)];
}
