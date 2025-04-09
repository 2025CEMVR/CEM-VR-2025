# Dockerfile
FROM unityci/editor:ubuntu-2021.3.8f1-linux-il2cpp-3.1.0 AS base

# Remove unnecessary files to reduce image size
RUN apt-get update && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Remove unused Unity components 
RUN rm -rf /opt/unity/Editor/Data/PlaybackEngines/AndroidPlayer \
           /opt/unity/Editor/Data/PlaybackEngines/iOSSupport \
           /opt/unity/Editor/Data/PlaybackEngines/TizenPlayer \
           /opt/unity/Editor/Data/PlaybackEngines/WebGLSupport

# Set the working directory
WORKDIR /workspace

# Default command
CMD ["/bin/bash"]
