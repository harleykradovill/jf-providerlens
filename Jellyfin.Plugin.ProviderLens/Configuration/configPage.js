(function () {
  var ProviderLensConfig = {
    pluginUniqueId: "ba6a4ff6-a27f-46e7-94ab-effb7dc39158",
  };

  var page = document.querySelector("#ProviderLensConfigPage");
  var tabs = page.querySelector('[is="emby-tabs"]');

  function getCheckedValues(name) {
    return Array.from(
      page.querySelectorAll('input[name="' + name + '"]:checked'),
    ).map(function (x) {
      return x.value;
    });
  }

  function setCheckedValues(name, values) {
    var set = new Set(values || []);
    page.querySelectorAll('input[name="' + name + '"]').forEach(function (x) {
      x.checked = set.has(x.value);
    });
  }

  function setActiveTab(index) {
    page.querySelectorAll(".tabContent").forEach(function (panel) {
      if (parseInt(panel.getAttribute("data-index"), 10) === index) {
        panel.classList.add("is-active");
      } else {
        panel.classList.remove("is-active");
      }
    });
  }

  function escapeHtml(value) {
    return String(value)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#39;");
  }

  function getLibraryId(item) {
    return String(item.ItemId || item.Id || "");
  }

  function getLibraryNameMap(libraries) {
    var map = new Map();
    (libraries || []).forEach(function (library) {
      map.set(getLibraryId(library), library.Name || getLibraryId(library));
    });
    return map;
  }

  function renderDashboard(snapshot, libraries, monitoredLibraryIds) {
    var updatedAt = page.querySelector("#DashboardUpdatedAt");
    var host = page.querySelector("#DashboardTables");

    var matches = Array.isArray(snapshot && snapshot.Matches)
      ? snapshot.Matches
      : [];
    var libraryNameMap = getLibraryNameMap(libraries);
    var monitoredIds = Array.isArray(monitoredLibraryIds)
      ? monitoredLibraryIds
      : [];

    var libraryIds = monitoredIds.length
      ? monitoredIds
      : Array.from(
          new Set(
            matches
              .map(function (match) {
                return match.LibraryId || "";
              })
              .filter(Boolean),
          ),
        );

    var updatedUtc =
      snapshot && snapshot.UpdatedUtc ? new Date(snapshot.UpdatedUtc) : null;
    if (updatedUtc && !Number.isNaN(updatedUtc.getTime())) {
      updatedAt.textContent = "Last updated: " + updatedUtc.toLocaleString();
    } else {
      updatedAt.textContent = "";
    }

    if (libraryIds.length === 0) {
      host.innerHTML =
        '<div class="fieldDescription">No monitored libraries are configured yet.</div>';
      return;
    }

    host.innerHTML = libraryIds
      .map(function (libraryId) {
        var libraryMatches = matches.filter(function (match) {
          return (match.LibraryId || "") === libraryId;
        });

        var libraryName =
          libraryNameMap.get(libraryId) || libraryId || "Unknown Library";

        if (libraryMatches.length === 0) {
          return (
            '<div class="verticalSection">' +
            "<h3>" +
            escapeHtml(libraryName) +
            "</h3>" +
            '<div class="fieldDescription">No matched titles found in this library.</div>' +
            "</div>"
          );
        }

        var rows = libraryMatches
          .map(function (match) {
            var providers = (match.Providers || [])
              .map(function (provider) {
                return provider.ProviderName || provider.ProviderId || "";
              })
              .filter(Boolean)
              .join(", ");

            return (
              "<tr>" +
              "<td>" +
              escapeHtml(match.Name || "") +
              "</td>" +
              "<td>" +
              escapeHtml(providers) +
              "</td>" +
              "</tr>"
            );
          })
          .join("");

        return (
          '<div class="verticalSection">' +
          "<h3>" +
          escapeHtml(libraryName) +
          "</h3>" +
          '<table class="detailTable">' +
          "<thead><tr><th>Media Title</th><th>Streaming Services</th></tr></thead>" +
          "<tbody>" +
          rows +
          "</tbody>" +
          "</table>" +
          "</div>"
        );
      })
      .join("");
  }

  function loadDashboard(monitoredLibraryIds, libraries) {
    return ApiClient.getJSON(ApiClient.getUrl("ProviderLens/Dashboard"))
      .then(function (snapshot) {
        renderDashboard(snapshot, libraries, monitoredLibraryIds);
      })
      .catch(function () {
        renderDashboard(
          { Matches: [], UpdatedUtc: null },
          libraries,
          monitoredLibraryIds,
        );
      });
  }

  function renderLibraries(items, selectedIds) {
    var host = page.querySelector("#LibrariesList");
    var selectedSet = new Set(selectedIds || []);

    host.innerHTML = items
      .map(function (item) {
        var id = item.ItemId || item.Id || "";
        var name = item.Name || id;
        var checked = selectedSet.has(id) ? " checked" : "";
        return (
          '<div class="checkboxContainer checkboxContainer-withDescription">' +
          '<label><input type="checkbox" is="emby-checkbox" name="libraryOption" value="' +
          escapeHtml(id) +
          '"' +
          checked +
          " /><span>" +
          escapeHtml(name) +
          "</span></label></div>"
        );
      })
      .join("");

    if (window.CustomElements && window.CustomElements.upgradeSubtree) {
      window.CustomElements.upgradeSubtree(host);
    }
  }

  function loadLibraries(selectedLibraryIds) {
    return ApiClient.getJSON(ApiClient.getUrl("Library/VirtualFolders"))
      .then(function (libraries) {
        var items = Array.isArray(libraries) ? libraries : [];
        renderLibraries(items, selectedLibraryIds);
        return items;
      })
      .catch(function () {
        renderLibraries([], selectedLibraryIds);
        return [];
      });
  }

  function parseTabIndex(value) {
    var index = parseInt(value, 10);
    return Number.isNaN(index) ? 0 : index;
  }

  function selectTab(index, updateControl) {
    setActiveTab(index);

    if (updateControl) {
      if (typeof tabs.selectedIndex === "function") {
        tabs.selectedIndex(index, false);
      } else if ("selectedIndex" in tabs) {
        tabs.selectedIndex = index;
      }
    }
  }

  tabs.addEventListener("click", function (e) {
    var button = e.target.closest(".emby-tab-button");
    if (!button) {
      return;
    }

    selectTab(parseTabIndex(button.getAttribute("data-index")), true);
  });

  tabs.addEventListener("tabchange", function (e) {
    var detail = e && e.detail ? e.detail : {};
    var index = Number.isInteger(detail.selectedIndex)
      ? detail.selectedIndex
      : Number.isInteger(detail.selectedTabIndex)
        ? detail.selectedTabIndex
        : parseTabIndex(detail.index);

    selectTab(index, false);
  });

  page.addEventListener("pageshow", function () {
    Dashboard.showLoadingMsg();

    ApiClient.getPluginConfiguration(ProviderLensConfig.pluginUniqueId)
      .then(function (config) {
        page.querySelector("#TmdbApiKey").value = config.TmdbApiKey || "";
        page.querySelector("#Country").value = (
          config.Country || ""
        ).toUpperCase();
        setCheckedValues("providerOption", config.SelectedProviders || []);

        var monitoredLibraryIds = config.MonitoredLibraryIds || [];
        return loadLibraries(monitoredLibraryIds).then(function (libraries) {
          return loadDashboard(monitoredLibraryIds, libraries);
        });
      })
      .finally(function () {
        tabs.selectedIndex(0, false);
        setActiveTab(0);
        Dashboard.hideLoadingMsg();
      });
  });

  page
    .querySelector("#ProviderLensConfigForm")
    .addEventListener("submit", function (e) {
      e.preventDefault();
      Dashboard.showLoadingMsg();

      ApiClient.getPluginConfiguration(ProviderLensConfig.pluginUniqueId)
        .then(function (config) {
          config.TmdbApiKey = page.querySelector("#TmdbApiKey").value.trim();
          config.Country = page
            .querySelector("#Country")
            .value.trim()
            .toUpperCase();
          config.SelectedProviders = getCheckedValues("providerOption");
          config.MonitoredLibraryIds = getCheckedValues("libraryOption");
          return ApiClient.updatePluginConfiguration(
            ProviderLensConfig.pluginUniqueId,
            config,
          );
        })
        .then(function (result) {
          Dashboard.processPluginConfigurationUpdateResult(result);
        })
        .finally(function () {
          Dashboard.hideLoadingMsg();
        });

      return false;
    });
})();
