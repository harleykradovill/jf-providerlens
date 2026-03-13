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
      })
      .catch(function () {
        renderLibraries([], selectedLibraryIds);
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
        return loadLibraries(config.MonitoredLibraryIds || []);
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
