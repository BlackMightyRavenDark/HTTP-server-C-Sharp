"use strict";

const nodeFileList = document.querySelector("#filelist");
const nodeCurrentPath = document.querySelector("#path");
let currentDirectory = "/";

async function getDirectoryFileList(directory) {
    const url = `/api/browse?${directory}`;
    try {
        const response = await fetch(url);
        const json = response.status === 200 ? await response.json() : null;
        return [response.status, json];
    } catch {
        return [400, null, "Failed to fetch"];
    }
}

function buildCurrentPath() {
    clearChildNodes(nodeCurrentPath);
    const trace = currentDirectory === '/' ? currentDirectory.split("/").slice(0, -1) : currentDirectory.split("/");
    trace.reduce((pth, element, id) => {
        const fullPath = id === 0 ? "/" : `${pth.endsWith("/") ? pth : `${pth}/`}${element}`;
        const nodePathDir = document.createElement("span");
        nodePathDir.textContent = id === 0 ? "ROOT" : element;
        nodePathDir.url = fullPath;
        nodePathDir.addEventListener("click", (e) => {
            clearChildNodes(nodeFileList);
            navigate(e.target.url);
        });
        nodeCurrentPath.appendChild(nodePathDir);
        return fullPath;
    }, "");
}

function clearChildNodes(node) {
    for (let i = node.childNodes.length - 1; i >= 0; --i) {
        node.removeChild(node.childNodes[i]);
    }
}

function parseResponse(json) {
    clearChildNodes(nodeFileList);
    json.forEach(element => {
        const nodeFileListItemAnchor = document.createElement("a");
        const nodeFileListItem = document.createElement("div");
        nodeFileListItem.textContent = element.name;
        const path = currentDirectory.endsWith("/") ? `${currentDirectory}${element.name}` : `${currentDirectory}/${element.name}`;
        if (element.type === "directory") {
            nodeFileListItem.classList.add("item-directory");
            nodeFileListItem.addEventListener("click", async (e) => {
                await navigate(path);
            });
        } else {
            nodeFileListItemAnchor.setAttribute("href", `@${encodeURIComponent(path)}`);
            nodeFileListItemAnchor.setAttribute("target", "_blank");
        }
        nodeFileListItemAnchor.appendChild(nodeFileListItem);
        nodeFileList.appendChild(nodeFileListItemAnchor);
    });
}

async function navigate(path) {
    const result = await getDirectoryFileList(encodeURIComponent(path));
    if (result[0] === 200) {
        currentDirectory = path;
        buildCurrentPath();
        parseResponse(result[1]);
    } else {
        console.error(`${result[0]} ${result[2]}`);
    }
}

navigate(currentDirectory);
