FROM ubuntu:16.04
COPY . /home/lightfantastic/
WORKDIR /home/lightfantastic/
RUN apt-get update && \
    apt-get install -y nano && \ 
    apt-get install -y curl && \
    apt-get install -y net-tools && \
    chmod u+s /bin/netstat &&\
    groupadd -g 999 lightfantastic && \
    useradd -r -u 999 -g lightfantastic lightfantastic && \
    chmod +x lightfantastic_server.x86_64 && \
    chown -R lightfantastic .
USER lightfantastic
ENTRYPOINT ["./lightfantastic_server.x86_64"]
