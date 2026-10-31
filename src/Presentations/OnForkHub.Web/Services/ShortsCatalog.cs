namespace OnForkHub.Web.Services;

using OnForkHub.Web.Models;

/// <summary>
/// Fixed catalog for the shorts feed. Open movies from the Blender Foundation published as torrents with
/// HTTP web seeds, so they play even when no other peer is online.
/// </summary>
public static class ShortsCatalog
{
    private const string BlenderCredit = "Blender Foundation · CC BY";

    public static IReadOnlyList<ShortVideo> Videos { get; } =
    [
        new(
            "Sintel",
            "Uma jovem atravessa terras geladas em busca do dragão que ela criou.",
            BlenderCredit,
            "https://webtorrent.io/torrents/sintel.torrent"
        ),
        new(
            "Big Buck Bunny",
            "Um coelho gigante e pacato decide dar o troco nos roedores que o atormentam.",
            BlenderCredit,
            "https://webtorrent.io/torrents/big-buck-bunny.torrent"
        ),
        new(
            "Tears of Steel",
            "Em Amsterdã, um grupo de cientistas tenta salvar o mundo de robôs destruidores.",
            BlenderCredit,
            "https://webtorrent.io/torrents/tears-of-steel.torrent"
        ),
        new(
            "Cosmos Laundromat",
            "Uma ovelha à beira do desespero recebe uma proposta de um vendedor misterioso.",
            BlenderCredit,
            "https://webtorrent.io/torrents/cosmos-laundromat.torrent"
        ),
    ];
}
